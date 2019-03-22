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
            WriteToLogger(logger, logMessage.Level, logMessage.Message);
        }

        public static void LogHandler<T1, T2>(ILogger logger, IConsumer<T1, T2> producer, LogMessage logMessage)
        {
            WriteToLogger(logger, logMessage.Level, logMessage.Message);
        }

        public static void ErrorHandler<T1, T2>(ILogger logger, IProducer<T1, T2> producer, Error error)
        {
            logger.Error("Error in Kafka producer: {error}", error);
        }

        public static void ErrorHandler<T1, T2>(ILogger logger, IConsumer<T1, T2> producer, Error error)
        {
            logger.Error("Error in Kafka consumer: {error}", error);
        }

        public static void OffsetsCommitted<T1, T2>(ILogger logger, IConsumer<T1, T2> producer, CommittedOffsets committedOffsets)
        {
            var offsetsByTopic = committedOffsets.Offsets
                .GroupBy(o => o.Topic)
                .Select(g => new { Topic = g.Key, Offsets = g.Select(o => $"{o.Partition.Value}={o.Offset.Value}").ToList() })
                .ToList();

            logger.Debug("Committed offsets: {@offsets}", offsetsByTopic);
        }

        public static void PartitionsRevoked<T1, T2>(ILogger logger, IConsumer<T1, T2> consumer, List<TopicPartitionOffset> partitions)
        {
            var partitionsByTopic = partitions
                .GroupBy(p => p.Topic)
                .Select(g => new { Topic = g.Key, Partitions = g.Select(p => p.Partition.Value) })
                .ToList();

            logger.Info("Revocation: {@partitions}", partitionsByTopic);
        }

        public static IEnumerable<TopicPartitionOffset> PartitionsAssigned<T1, T2>(ILogger logger, IConsumer<T1, T2> consumer, IEnumerable<TopicPartition> partitions, IPositionManager positionManager)
        {
            var partitionsByTopic = partitions
                .GroupBy(p => p.Topic)
                .Select(g => new { Topic = g.Key, Partitions = g.Select(p => p.Partition.Value) })
                .ToList();

            logger.Info("Assignment: {@partitions}", partitionsByTopic);

            var positions = partitions.GroupBy(p => p.Topic)
                .Select(g => new
                {
                    Topic = g.Key,
                    Partitions = g.Select(a => a.Partition.Value).ToList()
                })
                .SelectMany(a => AsyncHelpers.GetAsync(() => positionManager.Get(a.Topic, a.Partitions)))
                .ToList();

            var topicPartitionOffsets = positions
                .Select(p => p.Advance(1)) //< no need to read this again
                .Select(p => p.ToTopicPartitionOffset());

            return topicPartitionOffsets;
        }

        //public static void RebalanceHandler<T1, T2>(ILogger logger, IConsumer<T1, T2> consumer, RebalanceEvent rebalanceEvent, IPositionManager positionManager)
        //{
        //    var partitionsByTopic = rebalanceEvent.Partitions
        //        .GroupBy(p => p.Topic)
        //        .Select(g => new
        //        {
        //            Topic = g.Key,
        //            Partitions = g.Select(p => p.Partition.Value)
        //        })
        //        .ToList();

        //    if (rebalanceEvent.IsAssignment)
        //    {
        //        logger.Info("Assignment: {@partitions}", partitionsByTopic);

        //        var positions = rebalanceEvent.Partitions.GroupBy(p => p.Topic)
        //            .Select(g => new
        //            {
        //                Topic = g.Key,
        //                Partitions = g.Select(a => a.Partition.Value).ToList()
        //            })
        //            .SelectMany(a => AsyncHelpers.GetAsync(() => positionManager.Get(a.Topic, a.Partitions)))
        //            .ToList();

        //        var topicPartitionOffsets = positions
        //            .Select(p => p.Advance(1)) //< no need to read this again
        //            .Select(p => p.ToTopicPartitionOffset());

        //        consumer.Assign(topicPartitionOffsets);
        //    }
        //    else if (rebalanceEvent.IsRevocation)
        //    {
        //        logger.Info("Revocation: {@partitions}", partitionsByTopic);
        //    }
        //}

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