using System;
using Topos.Broker;

namespace Topos.Config
{
    public static class EventStoreConfigurer
    {
        public static ToposConsumerConfigurer EventBroker(this ToposConsumerConfigurer configurer, Action<StandardConfigurer<IEventBroker>> configure)
        {
            var standardConfigurer = StandardConfigurer<IEventBroker>.New(configurer);
            configure(standardConfigurer);
            return configurer;
        }

        public static ToposProducerConfigurer EventBroker(this ToposProducerConfigurer configurer, Action<StandardConfigurer<IEventBroker>> configure)
        {
            var standardConfigurer = StandardConfigurer<IEventBroker>.New(configurer);
            configure(standardConfigurer);
            return configurer;
        }
    }
}