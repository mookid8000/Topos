using System;
using Topos.Internals;
// ReSharper disable UnusedTypeParameter

namespace Topos.Config;

public class StandardConfigurer
{
    readonly Injectionist _injectionist;

    protected StandardConfigurer(Injectionist injectionist) => _injectionist = injectionist ?? throw new ArgumentNullException(nameof(injectionist));

    internal static Injectionist Open(ToposProducerConfigurer configurer) => configurer._injectionist;

    internal static Injectionist Open(ToposConsumerConfigurer configurer) => configurer._injectionist;

    public static Registrar<T> Open<T>(StandardConfigurer<T> configurer) => new Registrar<T>(configurer._injectionist);
}

public class StandardConfigurer<TService> : StandardConfigurer
{
    StandardConfigurer(Injectionist injectionist) : base(injectionist)
    {
    }

    internal static StandardConfigurer<TService> New(Injectionist injectionist) => new StandardConfigurer<TService>(injectionist);

    internal static StandardConfigurer<TService> New(ToposProducerConfigurer configurer)
    {
        var injectionist = Open(configurer);

        return New(injectionist);
    }

    internal static StandardConfigurer<TService> New(ToposConsumerConfigurer configurer)
    {
        var injectionist = Open(configurer);

        return New(injectionist);
    }
}