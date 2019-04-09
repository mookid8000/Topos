using System;
using System.Linq;
using Serilog;
using Topos.Serilog;
using Topos.Tests;

namespace Topos.Kafka.Tests
{
    public abstract class KafkaFixtureBase : ToposFixtureBase
    {
        protected string GetNewTopic()
        {
            var logger = Logger;

            return GetTopic(logger);
        }

        public static string GetTopic(ILogger logger)
        {
            using (var producer = new KafkaProducerImplementation(new SerilogLoggerFactory(logger), KafkaTestConfig.Address))
            using (var adminClient = producer.GetAdminClient())
            {
                var topics = adminClient
                    .GetMetadata(TimeSpan.FromSeconds(10))
                    .Topics.Select(topic => topic.Topic)
                    .Select(topic => topic.Split('-'))
                    .Where(parts => parts.Length == 2 && int.TryParse(parts[1], out _))
                    .Select(parts => int.Parse(parts[1]))
                    .ToList();

                var number = topics.Any() ? topics.Max() : 0;

                var topicName = $"testtopic-{number + 1}";

                logger.Information("Using topic named {topic}", topicName);

                return topicName;
            }
        }
    }
}