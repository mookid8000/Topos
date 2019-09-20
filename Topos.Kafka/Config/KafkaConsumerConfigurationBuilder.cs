using Confluent.Kafka;
using Topos.Internals;

namespace Topos.Config
{
    public class KafkaConsumerConfigurationBuilder
    {
        internal ConsumerConfig Apply(ConsumerConfig config)
        {
            var bootstrapServers = config.BootstrapServers;

            AzureEventHubsHelper.TrySetConnectionInfo(bootstrapServers, info =>
            {
                config.BootstrapServers = info.BootstrapServers;
                config.SaslUsername = info.SaslUsername;
                config.SaslPassword = info.SaslPassword;

                config.SessionTimeoutMs = 30000;
                config.SecurityProtocol = SecurityProtocol.SaslSsl;
                config.SaslMechanism = SaslMechanism.Plain;
            });

            return config;
        }
    }
}