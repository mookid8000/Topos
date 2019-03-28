using System;
using System.Collections.Generic;
using System.Linq;
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

        public static IEnumerable<TopicPartitionOffset> PartitionsAssigned<T1, T2>(ILogger logger, IConsumer<T1, T2> consumer, IEnumerable<TopicPartition> partitions, IPositionManager positionManager)
        {
            var partitionsList = partitions.ToList();

            if (!partitionsList.Any()) return Enumerable.Empty<TopicPartitionOffset>();

            var partitionsByTopic = partitionsList
                .GroupBy(p => p.Topic)
                .Select(g => new { Topic = g.Key, Partitions = g.Select(p => p.Partition.Value) })
                .ToList();

            logger.Info("Assignment: {@partitions}", partitionsByTopic);

            return partitionsList
                .Select(tp => new
                {
                    TopicPartition = tp,
                    Position = AsyncHelpers.GetAsync(() => positionManager.Get(tp.Topic, tp.Partition.Value))
                })
                .Select(a => a.Position?.Advance(1).ToTopicPartitionOffset() // either resume from the event following the last one successfully committedf
                             ?? a.TopicPartition.WithOffset(Offset.Beginning)); // or just resume from the beginning
        }

        public static void PartitionsRevoked<T1, T2>(ILogger logger, IConsumer<T1, T2> consumer, List<TopicPartitionOffset> partitions)
        {
            var partitionsList = partitions.ToList();

            if (!partitionsList.Any()) return;

            var partitionsByTopic = partitionsList
                .GroupBy(p => p.Topic)
                .Select(g => new { Topic = g.Key, Partitions = g.Select(p => p.Partition.Value) })
                .ToList();

            logger.Info("Revocation: {@partitions}", partitionsByTopic);
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