using System;
using Topos.Serialization;

namespace Topos.Config;

public static class SerializationConfigurationExtensions
{
    public static ToposConsumerConfigurer Serialization(this ToposConsumerConfigurer configurer, Action<StandardConfigurer<IMessageSerializer>> configure)
    {
        if (configurer == null) throw new ArgumentNullException(nameof(configurer));
        if (configure == null) throw new ArgumentNullException(nameof(configure));
        var standardConfigurer = StandardConfigurer<IMessageSerializer>.New(configurer);
        configure(standardConfigurer);
        return configurer;
    }

    public static ToposProducerConfigurer Serialization(this ToposProducerConfigurer configurer, Action<StandardConfigurer<IMessageSerializer>> configure)
    {
        if (configurer == null) throw new ArgumentNullException(nameof(configurer));
        if (configure == null) throw new ArgumentNullException(nameof(configure));
        var standardConfigurer = StandardConfigurer<IMessageSerializer>.New(configurer);
        configure(standardConfigurer);
        return configurer;
    }
}