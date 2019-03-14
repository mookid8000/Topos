using System;

namespace Topos.Config
{
    public static class Configure
    {
        public static ToposProducerConfigurer Producer(Action<StandardConfigurer<IToposProducerImplementation>> configure) => new ToposProducerConfigurer(configure);
        
        public static ToposConsumerConfigurer Consumer(Action<StandardConfigurer<IToposConsumerImplementation>> configure) => new ToposConsumerConfigurer(configure);
    }
}
