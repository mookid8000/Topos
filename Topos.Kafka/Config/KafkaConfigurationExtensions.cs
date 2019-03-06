using System.Collections.Generic;
using Topos.Logging;
using Topos.Producer;
using Topos.Routing;
using Topos.Serialization;

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
                    //return new KafkaProducer(loggerFactory, string.Join("; ", bootstrapServers));
                    var messageSerializer = c.Get<IMessageSerializer>();
                    var topicMapper = c.Get<ITopicMapper>();
                    return new DefaultToposProducer(messageSerializer, topicMapper);
                });

            return builder;
        }
    }
}