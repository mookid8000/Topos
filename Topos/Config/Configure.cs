using System;

namespace Topos.Config
{
    public static class Configure
    {
        public static ToposProducerConfigurer Producer(Action<StandardConfigurer<IToposProducer>> configure) => new ToposProducerConfigurer(configure);
        
        public static ToposConsumerConfigurer Consumer(Action<StandardConfigurer<IToposConsumer>> configure) => new ToposConsumerConfigurer(configure);
    }
}
