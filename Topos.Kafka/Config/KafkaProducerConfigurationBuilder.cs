using System;
using System.Collections.Generic;
using System.Linq;
using Confluent.Kafka;
using Topos.Internals;

namespace Topos.Config
{
    public class KafkaProducerConfigurationBuilder
    {
        /// <summary>
        /// Adds a <see cref="ProducerConfig"/> customizer to the builder. This provides the ability to customize and/or completely replace the configuration
        /// used to build the producer
        /// </summary>
        public static void AddCustomizer(KafkaProducerConfigurationBuilder builder, Func<ProducerConfig, ProducerConfig> customizer) => builder._customizers.Add(customizer);

        readonly List<Func<ProducerConfig, ProducerConfig>> _customizers = new List<Func<ProducerConfig, ProducerConfig>>();

        internal ProducerConfig Apply(ProducerConfig config)
        {
            var bootstrapServers = config.BootstrapServers;

            config = _customizers.Aggregate(config, (cfg, customize) => customize(cfg));

            AzureEventHubsHelper.TrySetConnectionInfo(bootstrapServers, info =>
            {
                config.BootstrapServers = info.BootstrapServers;
                config.SaslUsername = info.SaslUsername;
                config.SaslPassword = info.SaslPassword;

                config.RequestTimeoutMs = 60000;
                config.SecurityProtocol = SecurityProtocol.SaslSsl;
                config.SaslMechanism = SaslMechanism.Plain;
                config.EnableSslCertificateVerification = false;
            });

            return config;
        }
    }
}