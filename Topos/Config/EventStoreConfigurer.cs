using System;
using Topos.EventStore;

namespace Topos.Config
{
    public static class EventStoreConfigurer
    {
        public static ToposConsumerConfigurer EventStore(this ToposConsumerConfigurer configurer, Action<StandardConfigurer<IEventStore>> configure)
        {
            var standardConfigurer = StandardConfigurer<IEventStore>.New(configurer);
            configure(standardConfigurer);
            return configurer;
        }

        public static ToposProducerConfigurer EventStore(this ToposProducerConfigurer configurer, Action<StandardConfigurer<IEventStore>> configure)
        {
            var standardConfigurer = StandardConfigurer<IEventStore>.New(configurer);
            configure(standardConfigurer);
            return configurer;
        }
    }
}