using System;
using System.Linq;
using Topos.Serilog;
using Topos.Tests;

namespace Topos.Kafka.Tests
{
    public abstract class KafkaFixtureBase : ToposFixtureBase
    {
        protected string GetNewTopic()
        {
            using (var producer = new KafkaProducerImplementation(new SerilogLoggerFactory(Logger), KafkaTestConfig.Address))
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

                Logger.Information("Using topic named {topic}", topicName);

                return topicName;
            }


            //var topicName = $"topic-{new Random(DateTime.Now.GetHashCode()).Next(100)}";

            //Using(new TopicDeleter(topicName));

            //Logger.Information("Using temp topic {topic}", topicName);

            //return topicName;
        }
    }
}