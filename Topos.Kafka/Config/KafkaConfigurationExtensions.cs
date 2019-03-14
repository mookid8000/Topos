using System.Collections.Generic;
using System.Linq;
using Topos.Consumer;
using Topos.Kafka;
using Topos.Logging;
using Topos.Routing;
using Topos.Serialization;

namespace Topos.Config
{
    public static class KafkaConfigurationExtensions
    {
        public static KafkaProducerConfigurationBuilder UseKafka(this StandardConfigurer<IToposProducerImplementation> configurer, string bootstrapServer) => UseKafka(configurer, new[] {bootstrapServer});
        
        public static KafkaProducerConfigurationBuilder UseKafka(this StandardConfigurer<IToposConsumerImplementation> configurer, string bootstrapServer) => UseKafka(configurer, new[] {bootstrapServer});

        public static KafkaProducerConfigurationBuilder UseKafka(this StandardConfigurer<IToposProducerImplementation> configurer, IEnumerable<string> bootstrapServers)
        {
            var builder = new KafkaProducerConfigurationBuilder();

            StandardConfigurer.Open(configurer)
                .Register(c =>
                {
                    var loggerFactory = c.Get<ILoggerFactory>();
                    return new KafkaProducer(loggerFactory, string.Join(";", bootstrapServers));

                    ////return new KafkaProducer(loggerFactory, string.Join("; ", bootstrapServers));
                    //var messageSerializer = c.Get<IMessageSerializer>();
                    //var topicMapper = c.Get<ITopicMapper>();
                    //return new DefaultToposProducer(messageSerializer, topicMapper);
                });

            return builder;
        }

        public static KafkaProducerConfigurationBuilder UseKafka(this StandardConfigurer<IToposConsumerImplementation> configurer, IEnumerable<string> bootstrapServers)
        {
            var builder = new KafkaProducerConfigurationBuilder();

            StandardConfigurer.Open(configurer)
                .Register(c =>
                {
                    var loggerFactory = c.Get<ILoggerFactory>();
                    return new KafkaConsumer(loggerFactory, string.Join("; ", bootstrapServers), Enumerable.Empty<string>(),
                        "group", null);

                    //var messageSerializer = c.Get<IMessageSerializer>();
                    //var topicMapper = c.Get<ITopicMapper>();
                    //return new DefaultToposConsumer(messageSerializer, topicMapper);
                });

            return builder;
        }
    }
}