using System;

namespace Topos.Config
{
    public static class Configure
    {
        public static ToposProducerConfigurer Producer(Action<StandardConfigurer<IProducerImplementation>> configure) => new ToposProducerConfigurer(configure);
        
        public static ToposConsumerConfigurer Consumer(Action<StandardConfigurer<IConsumerImplementation>> configure) => new ToposConsumerConfigurer(configure);
    }
}
