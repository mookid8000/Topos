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
                .Select(g => new {Topic = g.Key, Offsets = g.Select(o => $"{o.Partition.Value}={o.Offset.Value}").ToList()})
                .ToList();

            logger.Debug("Committed offsets: {@offsets}", offsetsByTopic);
        }

        public static void RebalanceHandler<T1, T2>(ILogger logger, IConsumer<T1, T2> consumer,
            RebalanceEvent rebalanceEvent,
            Func<IEnumerable<Part>, Task> partitionsAssigned,
            Func<IEnumerable<Part>, Task> partitionsRevoked,
            IPositionManager positionManager)
        {
            var partitionsByTopic = rebalanceEvent.Partitions
                .GroupBy(p => p.Topic)
                .Select(g => new
                {
                    Topic = g.Key,
                    Partitions = g.Select(p => p.Partition.Value).ToArray()
                })
                .ToList();

            var parts = rebalanceEvent.Partitions
                .Select(p => new Part(p.Topic, p.Partition.Value));

            if (rebalanceEvent.IsAssignment)
            {
                logger.Info("Assignment: {partitions}", partitionsByTopic);

                partitionsAssigned(parts);
            }
            else if (rebalanceEvent.IsRevocation)
            {
                logger.Info("Revocation: {partitions}", partitionsByTopic);

                partitionsRevoked(parts);
            }
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