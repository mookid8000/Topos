using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Topos.Consumer;
using Topos.Logging;

namespace Topos.Internals
{
    static class Callbacks
    {
        public static void LogHandler<T1, T2>(ILogger logger, IProducer<T1, T2> producer, LogMessage logMessage)
        {
            WriteToLogger(logger, logMessage.Level, $"{logMessage.Name}/{logMessage.Facility}: {logMessage.Message}");
        }

        public static void LogHandler<T1, T2>(ILogger logger, IConsumer<T1, T2> producer, LogMessage logMessage)
        {
            WriteToLogger(logger, logMessage.Level, $"{logMessage.Name}/{logMessage.Facility}: {logMessage.Message}");
        }

        public static void ErrorHandler<T1, T2>(ILogger logger, IProducer<T1, T2> producer, Error error)
        {
            // the producer
            // [Error] Error in Kafka producer: Error { Code: Local_AllBrokersDown, IsFatal: False, Reason: "1/1 brokers are down", IsError: True, IsLocalError: True, IsBrokerError: False }

            logger.Error("Error in Kafka producer: {@error}", error);
        }

        public static void ErrorHandler<T1, T2>(ILogger logger, IConsumer<T1, T2> producer, Error error)
        {
            logger.Error("Error in Kafka consumer: {@error}", error);
        }

        public static IEnumerable<TopicPartitionOffset> PartitionsAssigned(
            ILogger logger,
            IEnumerable<TopicPartition> partitions,
            IPositionManager positionManager,
            Func<ConsumerContext, IEnumerable<TopicPartition>, Task> partitionsAssignedHandler,
            ConsumerContext context
        )
        {
            var partitionsList = partitions.ToList();

            if (!partitionsList.Any()) return Enumerable.Empty<TopicPartitionOffset>();

            var partitionsByTopic = partitionsList
                .GroupBy(p => p.Topic)
                .Select(g => new { Topic = g.Key, Partitions = g.Select(p => p.Partition.Value) })
                .ToList();

            logger.Info("Assignment: {@partitions}", partitionsByTopic);

            if (partitionsAssignedHandler != null)
            {
                AsyncHelpers.RunSync(() => partitionsAssignedHandler(context, partitionsList));
            }

            return AsyncHelpers.GetAsync(async () =>
            {
                var results = await partitionsList
                    .Select(async tp => new
                    {
                        TopicPartition = tp,
                        Position = await positionManager.Get(tp.Topic, tp.Partition.Value)
                    })
                    .ToListAsync();

                return results
                    .Select(a =>
                    {
                        if (a.Position.IsDefault) return a.TopicPartition.WithOffset(Offset.Beginning);
                        if (a.Position.IsOnlyNew) return a.TopicPartition.WithOffset(Offset.Stored);

                        return a.Position.Advance(1).ToTopicPartitionOffset();

                        //return (isDefault: a.Position.IsDefault, isOnlyNew: a.Position.IsOnlyNew) switch
                        //{
                        //    (isDefault: true, _) => a.TopicPartition.WithOffset(Offset.Beginning),
                        //    (_, isOnlyNew: true) => a.TopicPartition.WithOffset(Offset.Stored),

                        //    _ => a.Position.Advance(1).ToTopicPartitionOffset()
                        //};

                        //return
                        //    a.Position?.Advance(1)
                        //        .ToTopicPartitionOffset() // either resume from the event following the last one successfully committed
                        //    ?? a.TopicPartition.WithOffset(Offset.Beginning);
                    });
            });
        }

        public static void PartitionsRevoked(ILogger logger,
            List<TopicPartitionOffset> partitions,
            IConsumerDispatcher consumerDispatcher,
            Func<ConsumerContext, IEnumerable<TopicPartition>, Task> partitionsRevokedHandler,
            ConsumerContext context)
        {
            var partitionsList = partitions.ToList();

            if (!partitionsList.Any()) return;

            AsyncHelpers.RunSync(async () =>
            {
                var partitionsByTopic = partitionsList
                    .GroupBy(p => p.Topic)
                    .Select(g => new { Topic = g.Key, Partitions = g.Select(p => p.Partition.Value) })
                    .ToList();

                logger.Info("Revocation: {@partitions}", partitionsByTopic);

                if (partitionsRevokedHandler != null)
                {
                    await partitionsRevokedHandler(context, partitionsList.Select(p => p.TopicPartition));
                }

                foreach (var revocation in partitionsByTopic)
                {
                    await consumerDispatcher.Revoke(revocation.Topic, revocation.Partitions);
                }
            });
        }

        static void WriteToLogger(ILogger logger, SyslogLevel level, string message)
        {
            switch (level)
            {
                case SyslogLevel.Emergency:
                case SyslogLevel.Alert:
                case SyslogLevel.Critical:
                case SyslogLevel.Error:
                    logger.Error(message);
                    break;
                case SyslogLevel.Warning:
                case SyslogLevel.Notice:
                    logger.Warn(message);
                    break;
                case SyslogLevel.Info:
                    logger.Info(message);
                    break;
                case SyslogLevel.Debug:
                    logger.Debug(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, "Unknown log level");
            }
        }
    }
}