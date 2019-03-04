using System;
using Topos.Logging;

namespace Topos.Config
{
    public static class LoggingConfigurer
    {
        public static ToposConsumerConfigurer Logging(this ToposConsumerConfigurer configurer, Action<StandardConfigurer<ILoggerFactory>> configure)
        {
            var standardConfigurer = StandardConfigurer<ILoggerFactory>.New(configurer);
            configure(standardConfigurer);
            return configurer;
        }

        public static ToposProducerConfigurer Logging(this ToposProducerConfigurer configurer, Action<StandardConfigurer<ILoggerFactory>> configure)
        {
            var standardConfigurer = StandardConfigurer<ILoggerFactory>.New(configurer);
            configure(standardConfigurer);
            return configurer;
        }
    }
}