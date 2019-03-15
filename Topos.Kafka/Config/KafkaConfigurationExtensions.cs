using System.Collections.Generic;
using Topos.Consumer;
using Topos.Kafka;
using Topos.Logging;

namespace Topos.Config
{
    public static class KafkaConfigurationExtensions
    {
        public static KafkaProducerConfigurationBuilder UseKafka(this StandardConfigurer<IProducerImplementation> configurer, params string[] bootstrapServer) => UseKafka(configurer, (IEnumerable<string>)bootstrapServer);

        public static KafkaProducerConfigurationBuilder UseKafka(this StandardConfigurer<IConsumerImplementation> configurer, params string[] bootstrapServer) => UseKafka(configurer, (IEnumerable<string>)bootstrapServer);

        public static KafkaProducerConfigurationBuilder UseKafka(this StandardConfigurer<IProducerImplementation> configurer, IEnumerable<string> bootstrapServers)
        {
            var builder = new KafkaProducerConfigurationBuilder();

            StandardConfigurer.Open(configurer)
                .Register(c =>
                {
                    var loggerFactory = c.Get<ILoggerFactory>();

                    return new KafkaProducerImplementation(loggerFactory, string.Join(";", bootstrapServers));
                });

            return builder;
        }

        public static KafkaProducerConfigurationBuilder UseKafka(this StandardConfigurer<IConsumerImplementation> configurer, IEnumerable<string> bootstrapServers)
        {
            var builder = new KafkaProducerConfigurationBuilder();

            StandardConfigurer.Open(configurer)
                .Register(c =>
                {
                    var loggerFactory = c.Get<ILoggerFactory>();
                    var topics = c.Get<Topics>();
                    var consumerDispatcher = c.Get<IConsumerDispatcher>();

                    return new KafkaConsumerImplementation(loggerFactory, string.Join("; ", bootstrapServers), topics, "group", (evt, token) => consumerDispatcher.Dispatch(evt));
                });

            return builder;
        }
    }
}