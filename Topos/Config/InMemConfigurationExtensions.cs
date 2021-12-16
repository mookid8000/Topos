using Topos.Consumer;
using Topos.InMem;
using Topos.Logging;

namespace Topos.Config;

public static class InMemConfigurationExtensions
{
    public static void UseInMemory(this StandardConfigurer<IProducerImplementation> configurer, InMemEventBroker eventBroker)
    {
        StandardConfigurer.Open(configurer)
            .Register(_ => new InMemProducerImplementation(eventBroker));
    }

    public static void UseInMemory(this StandardConfigurer<IConsumerImplementation> configurer, InMemEventBroker eventBroker)
    {
        StandardConfigurer.Open(configurer)
            .Register(c =>
            {
                var loggerFactory = c.Get<ILoggerFactory>();
                var topics = c.Has<Topics>() ? c.Get<Topics>() : new Topics();
                var consumerDispatcher = c.Get<IConsumerDispatcher>();
                var consumerContext = c.Get<ConsumerContext>();

                return new InMemConsumerImplementation(eventBroker, loggerFactory, topics, consumerDispatcher, consumerContext);
            });
    }

    public static void StoreInMemory(this StandardConfigurer<IPositionManager> configurer, InMemPositionsStorage positionsStorage = null)
    {
        var registrar = StandardConfigurer.Open(configurer);

        registrar.Register(_ => new InMemPositionsManager(positionsStorage ?? new InMemPositionsStorage()));
    }
}