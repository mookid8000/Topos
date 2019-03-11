using System;
using Serilog;
using Topos.Tests;

namespace Topos.Kafka.Tests
{
    public abstract class KafkaFixtureBase : ToposFixtureBase
    {
        static readonly ILogger Logger = Log.ForContext<KafkaFixtureBase>();

        protected string GetNewTopic()
        {
            var topicName = $"topic-{new Random(DateTime.Now.GetHashCode()).Next(100)}";

            Using(new TopicDeleter(topicName));

            Logger.Information("Using temp topic {topic}", topicName);

            return topicName;
        }
    }
}