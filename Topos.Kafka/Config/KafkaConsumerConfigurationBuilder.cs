using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Topos.Consumer;
using Topos.Internals;

// ReSharper disable RedundantDefaultMemberInitializer

namespace Topos.Config;

public class KafkaConsumerConfigurationBuilder
{
    static readonly Func<ConsumerContext, IEnumerable<TopicPartition>, Task>[] EmptyList = Array.Empty<Func<ConsumerContext, IEnumerable<TopicPartition>, Task>>();

    /// <summary>
    /// Adds a <see cref="ConsumerConfig"/> customizer to the builder. This provides the ability to customize and/or completely replace the configuration
    /// used to build the consumer
    /// </summary>
    public static void AddCustomizer(KafkaConsumerConfigurationBuilder builder, Func<ConsumerConfig, ConsumerConfig> customizer) => builder._customizers.Add(customizer);

    readonly List<Func<ConsumerConfig, ConsumerConfig>> _customizers = new();

    /// <summary>
    /// Registers the given <paramref name="handler"/> to be invoked when a topic/partition assignment occurs
    /// </summary>
    public KafkaConsumerConfigurationBuilder OnPartitionsAssigned(Func<ConsumerContext, IEnumerable<TopicPartition>, Task> handler)
    {
        OnPartitionsAssignedEvent += handler ?? throw new ArgumentNullException(nameof(handler));
        return this;
    }

    /// <summary>
    /// Registers the given <paramref name="handler"/> to be invoked when previously assigned topics/partitions are revoked
    /// </summary>
    public KafkaConsumerConfigurationBuilder OnPartitionsRevoked(Func<ConsumerContext, IEnumerable<TopicPartition>, Task> handler)
    {
        OnPartitionsRevokedEvent += handler ?? throw new ArgumentNullException(nameof(handler));
        return this;
    }

    /// <summary>
    /// Sets the delay the consumer implementation will wait before re-initializing, after having its patitions revoked
    /// </summary>
    public KafkaConsumerConfigurationBuilder SetChilldownOnRevocationDelay(TimeSpan delay)
    {
        ChilldownOnRevocationDelay = delay;
        return this;
    }

    internal TimeSpan ChilldownOnRevocationDelay { get; private set; }

    internal ConsumerConfig Apply(ConsumerConfig config)
    {
        var bootstrapServers = config.BootstrapServers;

        config = _customizers.Aggregate(config, (cfg, customize) => customize(cfg));

        AzureEventHubsHelper.TrySetConnectionInfo(bootstrapServers, info =>
        {
            config.BootstrapServers = info.BootstrapServers;
            config.SaslUsername = info.SaslUsername;
            config.SaslPassword = info.SaslPassword;

            config.SessionTimeoutMs = 30000;
            config.SecurityProtocol = SecurityProtocol.SaslSsl;
            config.SaslMechanism = SaslMechanism.Plain;
            config.EnableSslCertificateVerification = false;
        });

        return config;
    }

    internal event Func<ConsumerContext, IEnumerable<TopicPartition>, Task> OnPartitionsAssignedEvent;

    internal event Func<ConsumerContext, IEnumerable<TopicPartition>, Task> OnPartitionsRevokedEvent;

    internal Func<ConsumerContext, IEnumerable<TopicPartition>, Task> GetPartitionsAssignedHandler()
    {
        var handlers = GetHandlers(OnPartitionsAssignedEvent);

        return async (context, partitions) =>
        {
            var tasks = handlers.Select(handler => handler(context, partitions));

            await Task.WhenAll(tasks);
        };
    }

    internal Func<ConsumerContext, IEnumerable<TopicPartition>, Task> GetPartitionsRevokedHandler()
    {
        var handlers = GetHandlers(OnPartitionsRevokedEvent);

        return async (context, partitions) =>
        {
            var tasks = handlers.Select(handler => handler(context, partitions));

            await Task.WhenAll(tasks);
        };
    }

    static Func<ConsumerContext, IEnumerable<TopicPartition>, Task>[] GetHandlers(Func<ConsumerContext, IEnumerable<TopicPartition>, Task> @event)
    {
        return @event?.GetInvocationList()
                   .Cast<Func<ConsumerContext, IEnumerable<TopicPartition>, Task>>()
                   .ToArray()

               ?? EmptyList;
    }
}