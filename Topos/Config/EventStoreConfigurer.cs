using System;

namespace Topos.Config
{
    public static class EventStoreConfigurer
    {
        public static ToposProducerConfigurer EventBroker(this ToposProducerConfigurer configurer, Action<StandardConfigurer<IToposProducer>> configure)
        {
            var standardConfigurer = StandardConfigurer<IToposProducer>.New(configurer);
            configure(standardConfigurer);
            return configurer;
        }

        public static ToposConsumerConfigurer EventBroker(this ToposConsumerConfigurer configurer, Action<StandardConfigurer<IToposConsumer>> configure)
        {
            var standardConfigurer = StandardConfigurer<IToposConsumer>.New(configurer);
            configure(standardConfigurer);
            return configurer;
        }
    }
}