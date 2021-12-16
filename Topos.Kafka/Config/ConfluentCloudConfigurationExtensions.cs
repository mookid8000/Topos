using Confluent.Kafka;

namespace Topos.Config;

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
        KafkaProducerConfigurationBuilder.AddCustomizer(builder, config =>
        {
            config.SaslUsername = key;
            config.SaslPassword = secret;

            config.RequestTimeoutMs = 60000;
            config.SecurityProtocol = SecurityProtocol.SaslSsl;
            config.SaslMechanism = SaslMechanism.Plain;
            config.EnableSslCertificateVerification = false;
            config.SocketKeepaliveEnable = true;

            return config;
        });

        return builder;
    }

    /// <summary>
    /// Configures the Kafka client with good defaults for connecting to Confluent Cloud
    /// </summary>
    public static KafkaConsumerConfigurationBuilder WithConfluentCloud(this KafkaConsumerConfigurationBuilder builder, string key, string secret)
    {
        KafkaConsumerConfigurationBuilder.AddCustomizer(builder, config =>
        {
            config.SaslUsername = key;
            config.SaslPassword = secret;

            config.SessionTimeoutMs = 45000;
            config.SecurityProtocol = SecurityProtocol.SaslSsl;
            config.SaslMechanism = SaslMechanism.Plain;
            config.EnableSslCertificateVerification = false;
            config.SocketKeepaliveEnable = true;

            return config;
        });

        return builder;
    }
}