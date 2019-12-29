using Confluent.Kafka;

namespace Topos.Config
{
    /// <summary>
    /// Extensions for connecting to Confluent Cloud
    /// </summary>
    public static class ConfluentCloudConfigurationExtensions
    {
        /// <summary>
        /// Configures the Kafka client with good defaults for connecting to Confluent Cloud
        /// </summary>
        public static KafkaProducerConfigurationBuilder WithConfluentCloud(this KafkaProducerConfigurationBuilder builder, string key, string secret)
        {
            builder.AddCustomizer(config =>
            {
                config.SaslUsername = key;
                config.SaslPassword = secret;

                config.RequestTimeoutMs = 60000;
                config.SecurityProtocol = SecurityProtocol.SaslSsl;
                config.SaslMechanism = SaslMechanism.Plain;
                config.EnableSslCertificateVerification = true;

                return config;
            });

            return builder;
        }

        /// <summary>
        /// Configures the Kafka client with good defaults for connecting to Confluent Cloud
        /// </summary>
        public static KafkaConsumerConfigurationBuilder WithConfluentCloud(this KafkaConsumerConfigurationBuilder builder, string key, string secret)
        {
            builder.AddCustomizer(config =>
            {
                config.SaslUsername = key;
                config.SaslPassword = secret;

                config.SessionTimeoutMs = 10000;
                config.SecurityProtocol = SecurityProtocol.SaslSsl;
                config.SaslMechanism = SaslMechanism.Plain;
                config.EnableSslCertificateVerification = true;

                return config;
            });

            return builder;
        }
    }
}