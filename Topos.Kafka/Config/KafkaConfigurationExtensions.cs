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
                    var group = c.Get<GroupId>();
                    var consumerDispatcher = c.Get<IConsumerDispatcher>();
                    var positionManager = c.Get<IPositionManager>(errorMessage: @"The Kafka consumer needs access to a positions manager, so it can figure out which offsets to pick up from when starting up.");

                    return new KafkaConsumerImplementation(
                        loggerFactory: loggerFactory,
                        address: string.Join("; ", bootstrapServers),
                        topics: topics,
                        group: group.Id,
                        consumerDispatcher: consumerDispatcher,
                        positionManager: positionManager
                    );
                });

            return builder;
        }
    }
}