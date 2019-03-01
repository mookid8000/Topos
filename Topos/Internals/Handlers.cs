using System.Linq;
using Confluent.Kafka;
using Serilog;

namespace Topos.Internals
{
    static class Handlers
    {
        public static void LogHandler<T1, T2>(ILogger logger, IProducer<T1, T2> producer, LogMessage logMessage)
        {
            logger.Write(logMessage.Level.ToSerilogLevel(), logMessage.Message);
        }

        public static void ErrorHandler<T1, T2>(ILogger logger, IProducer<T1, T2> producer, Error error)
        {
            logger.Error("Error in Kafka producer: {@error}", error);
        }

        public static void LogHandler<T1, T2>(ILogger logger, IConsumer<T1, T2> producer, LogMessage logMessage)
        {
            logger.Write(logMessage.Level.ToSerilogLevel(), logMessage.Message);
        }

        public static void ErrorHandler<T1, T2>(ILogger logger, IConsumer<T1, T2> producer, Error error)
        {
            logger.Error("Error in Kafka consumer: {@error}", error);
        }

        public static void OffsetsCommitted<T1, T2>(ILogger logger, IConsumer<T1, T2> producer, CommittedOffsets committedOffsets)
        {
            var offsetsByTopic = committedOffsets.Offsets.GroupBy(o => o.Topic)
                .Select(g => new { Topic = g.Key, Offsets = g.Select(o => $"{o.Partition.Value}={o.Offset.Value}") });

            logger.Verbose("Committed offsets: {@offsets}", offsetsByTopic);
        }

        public static void RebalanceHandler<T1, T2>(ILogger logger, IConsumer<T1, T2> consumer, RebalanceEvent rebalanceEvent)
        {
            var partitiongByTopic = rebalanceEvent.Partitions.GroupBy(p => p.Topic)
                .Select(g => new
                {
                    Topic = g.Key,
                    Partitions = g.Select(p => p.Partition.Value).ToArray()
                });

            if (rebalanceEvent.IsAssignment)
            {
                logger.Information("Assignment: {@partitions}", partitiongByTopic);
            }
            else if (rebalanceEvent.IsRevocation)
            {
                logger.Information("Revocation: {@partitions}", partitiongByTopic);
            }
        }

    }
}