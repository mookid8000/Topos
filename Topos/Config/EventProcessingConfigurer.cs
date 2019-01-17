using System;
using Topos.EventProcessing;

namespace Topos.Config
{
    public static class EventProcessingConfigurer
    {
        public static ToposConsumerConfigurer EventProcessing(this ToposConsumerConfigurer configurer, Action<StandardConfigurer<IEventProcessor>> configure)
        {
            var standardConfigurer = StandardConfigurer<IEventProcessor>.New(configurer);
            configure(standardConfigurer);
            return configurer;
        }
    }
}