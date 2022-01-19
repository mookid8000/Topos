﻿using System.Collections.Generic;
using Topos.Consumer;
using Topos.Internals;
using Topos.Kafka;
using Topos.Logging;
// ReSharper disable ArgumentsStyleAnonymousFunction
// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleStringLiteral

namespace Topos.Config;

public static class KafkaConfigurationExtensions
{
    public static KafkaProducerConfigurationBuilder UseKafka(this StandardConfigurer<IProducerImplementation> configurer, params string[] bootstrapServer) => UseKafka(configurer, (IEnumerable<string>)bootstrapServer);

    public static KafkaProducerConfigurationBuilder UseKafka(this StandardConfigurer<IProducerImplementation> configurer, IEnumerable<string> bootstrapServers)
    {
        var builder = new KafkaProducerConfigurationBuilder();

        StandardConfigurer.Open(configurer)
            .Register(c => RegisterProducerImplementation(bootstrapServers, c, builder));

        return builder;
    }

    public static KafkaConsumerConfigurationBuilder UseKafka(this StandardConfigurer<IConsumerImplementation> configurer, params string[] bootstrapServer) => UseKafka(configurer, (IEnumerable<string>)bootstrapServer);

    public static KafkaConsumerConfigurationBuilder UseKafka(this StandardConfigurer<IConsumerImplementation> configurer, IEnumerable<string> bootstrapServers)
    {
        var builder = new KafkaConsumerConfigurationBuilder();

        StandardConfigurer.Open(configurer)
            .Register(c =>
            {
                var loggerFactory = c.Get<ILoggerFactory>();
                var topics = c.Has<Topics>() ? c.Get<Topics>() : new Topics();
                var group = c.Get<GroupId>();
                var consumerDispatcher = c.Get<IConsumerDispatcher>();
                var positionManager = c.Get<IPositionManager>(errorMessage: @"The Kafka consumer needs access to a positions manager, so it can figure out which offsets to pick up from when starting up.");
                var consumerContext = c.Get<ConsumerContext>();
                var partitionsAssignedHandler = builder.GetPartitionsAssignedHandler();
                var partitionsRevokedHandler = builder.GetPartitionsRevokedHandler();
                var startPosition = c.Has<PositionsStorageConfigurationExtensions.ExplicitlySetInitialPosition>() ? c.Get<PositionsStorageConfigurationExtensions.ExplicitlySetInitialPosition>().Position : StartFromPosition.Beginning;

                return new KafkaConsumerImplementation(
                    loggerFactory: loggerFactory,
                    address: string.Join("; ", bootstrapServers),
                    topics: topics,
                    group: group.Id,
                    consumerDispatcher: consumerDispatcher,
                    positionManager: positionManager,
                    context: consumerContext,
                    configurationCustomizer: config => builder.Apply(config),
                    partitionsAssignedHandler: partitionsAssignedHandler,
                    partitionsRevokedHandler: partitionsRevokedHandler,
                    startPosition: startPosition
                );
            });

        return builder;
    }

    static IProducerImplementation RegisterProducerImplementation(IEnumerable<string> bootstrapServers, IResolutionContext c, KafkaProducerConfigurationBuilder builder)
    {
        var loggerFactory = c.Get<ILoggerFactory>();

        return new KafkaProducerImplementation(
            loggerFactory: loggerFactory,
            address: string.Join(";", bootstrapServers),
            configurationCustomizer: builder.Apply
        );
    }
}