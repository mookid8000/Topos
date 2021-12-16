using System;
using System.Linq;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Serilog;
using Topos.Internals;
using Topos.Serilog;
using Topos.Tests;

namespace Topos.Kafka.Tests;

public abstract class KafkaFixtureBase : ToposFixtureBase
{
    protected string GetNewTopic(int numberOfPartitions = 1)
    {
        var logger = Logger;

        return GetTopic(logger, numberOfPartitions);
    }

    public static string GetTopic(ILogger logger, int numberOfPartitions = 1)
    {
        string result = null;

        WithAdminClient(logger, adminClient =>
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

            result = topicName;

            var topicSpecification = new TopicSpecification{Name = topicName, NumPartitions=numberOfPartitions,ReplicationFactor=1};

            adminClient
                .CreateTopicsAsync(new[] {topicSpecification})
                .Wait();
        });

        return result;
    }

    static void WithAdminClient(ILogger logger, Action<IAdminClient> callback)
    {
        using var producer = new KafkaProducerImplementation(new SerilogLoggerFactory(logger), KafkaTestConfig.Address, configurationCustomizer: ConfigurationCustomizer);
            
        using var adminClient = producer.GetAdminClient();
            
        callback(adminClient);
    }

    static ProducerConfig ConfigurationCustomizer(ProducerConfig config)
    {
        AzureEventHubsHelper.TrySetConnectionInfo(config.BootstrapServers, info =>
        {
            config.BootstrapServers = info.BootstrapServers;
            config.SaslUsername = info.SaslUsername;
            config.SaslPassword = info.SaslPassword;
        });

        return config;
    }
}