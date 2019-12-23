using System;
using System.Collections.Generic;
using System.Linq;
using Confluent.Kafka;
using Topos.Internals;

namespace Topos.Config
{
    public class KafkaProducerConfigurationBuilder
    {
        readonly List<Func<ProducerConfig, ProducerConfig>> _customizers = new List<Func<ProducerConfig, ProducerConfig>>();

        internal void AddCustomizer(Func<ProducerConfig, ProducerConfig> customizer) => _customizers.Add(customizer);

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