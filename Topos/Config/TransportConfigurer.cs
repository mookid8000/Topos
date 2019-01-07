using System;
using Topos.Transport;

namespace Topos.Config
{
    public static class TransportConfigurer
    {
        public static ToposConsumerConfigurer Transport(this ToposConsumerConfigurer configurer, Action<StandardConfigurer<ITransport>> configure)
        {

            return configurer;
        }

        public static ToposProducerConfigurer Transport(this ToposProducerConfigurer configurer, Action<StandardConfigurer<ITransport>> configure)
        {

            return configurer;
        }
    }
}