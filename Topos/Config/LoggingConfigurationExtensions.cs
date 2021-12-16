using System;
using Topos.Logging;

namespace Topos.Config;

public static class LoggingConfigurationExtensions
{
    public static ToposConsumerConfigurer Logging(this ToposConsumerConfigurer configurer, Action<StandardConfigurer<ILoggerFactory>> configure)
    {
        if (configurer == null) throw new ArgumentNullException(nameof(configurer));
        if (configure == null) throw new ArgumentNullException(nameof(configure));
        var standardConfigurer = StandardConfigurer<ILoggerFactory>.New(configurer);
        configure(standardConfigurer);
        return configurer;
    }

    public static ToposProducerConfigurer Logging(this ToposProducerConfigurer configurer, Action<StandardConfigurer<ILoggerFactory>> configure)
    {
        if (configurer == null) throw new ArgumentNullException(nameof(configurer));
        if (configure == null) throw new ArgumentNullException(nameof(configure));
        var standardConfigurer = StandardConfigurer<ILoggerFactory>.New(configurer);
        configure(standardConfigurer);
        return configurer;
    }
}