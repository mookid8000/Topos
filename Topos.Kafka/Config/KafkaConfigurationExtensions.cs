using System.Collections.Generic;
using Topos.Kafka;
using Topos.Logging;

namespace Topos.Config
{
    public static class KafkaConfigurationExtensions
    {
        public static KafkaProducerConfigurationBuilder UseKafka(this StandardConfigurer<IToposProducer> configurer, string bootstrapServer) => UseKafka(configurer, new[] {bootstrapServer});

        public static KafkaProducerConfigurationBuilder UseKafka(this StandardConfigurer<IToposProducer> configurer, IEnumerable<string> bootstrapServers)
        {
            var builder = new KafkaProducerConfigurationBuilder();

            StandardConfigurer.Open(configurer)
                .Register(c =>
                {
                    var loggerFactory = c.Get<ILoggerFactory>();
                    return new KafkaProducer(loggerFactory, string.Join("; ", bootstrapServers));
                })

            return builder;
        }
    }
}