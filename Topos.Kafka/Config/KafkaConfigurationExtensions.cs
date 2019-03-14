using System.Collections.Generic;
using System.Threading;
using Topos.Kafka;
using Topos.Logging;

namespace Topos.Config
{
    public static class KafkaConfigurationExtensions
    {
        public static KafkaProducerConfigurationBuilder UseKafka(this StandardConfigurer<IProducerImplementation> configurer, string bootstrapServer) => UseKafka(configurer, new[] { bootstrapServer });

        public static KafkaProducerConfigurationBuilder UseKafka(this StandardConfigurer<IConsumerImplementation> configurer, string bootstrapServer) => UseKafka(configurer, new[] { bootstrapServer });

        public static KafkaProducerConfigurationBuilder UseKafka(this StandardConfigurer<IProducerImplementation> configurer, IEnumerable<string> bootstrapServers)
        {
            var builder = new KafkaProducerConfigurationBuilder();

            StandardConfigurer.Open(configurer)
                .Register(c =>
                {
                    var loggerFactory = c.Get<ILoggerFactory>();

                    return new KafkaProducer(loggerFactory, string.Join(";", bootstrapServers));
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
                    var handlers = c.Get<Handlers>();

                    return new KafkaConsumer(loggerFactory, string.Join("; ", bootstrapServers), topics, "group", async (evt, position, token) =>
                    {
                    });
                });

            return builder;
        }
    }
}