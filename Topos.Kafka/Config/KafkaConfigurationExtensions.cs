using System.Collections.Generic;
using Topos.Consumer;
using Topos.Kafka;
using Topos.Logging;
// ReSharper disable ArgumentsStyleAnonymousFunction

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
                    var topics = c.Has<Topics>() ? c.Get<Topics>() : new Topics();
                    var consumerDispatcher = c.Get<IConsumerDispatcher>();
                    var positionManager = c.Get<IPositionManager>();

                    return new KafkaConsumerImplementation(
                        loggerFactory: loggerFactory,
                        address: string.Join("; ", bootstrapServers),
                        topics: topics,
                        group: "group",
                        eventHandler: (evt, token) => consumerDispatcher.Dispatch(evt),
                        positionManager: positionManager
                    );
                });

            return builder;
        }
    }
}