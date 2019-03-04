using System.Collections.Generic;
using Topos.Broker;

namespace Topos.Config
{
    public static class KafkaConfigurationExtensions
    {
        public static KafkaProducerConfigurationBuilder UseKafka(this StandardConfigurer<IEventBroker> configurer, string bootstrapServer) => UseKafka(configurer, new[] {bootstrapServer});

        public static KafkaProducerConfigurationBuilder UseKafka(this StandardConfigurer<IEventBroker> configurer, IEnumerable<string> bootstrapServers)
        {
            var builder = new KafkaProducerConfigurationBuilder();

            //StandardConfigurer.Open(configurer)
            //    .Register(c => new Kafka)

            return builder;
        }
    }
}