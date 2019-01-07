using System;
using Topos.Internals;
using Topos.Transport;

namespace Topos.Config
{
    public class StandardConfigurer<TService>
    {
        readonly Injectionist _injectionist;

        StandardConfigurer(Injectionist injectionist) => _injectionist = injectionist ?? throw new ArgumentNullException(nameof(injectionist), 
                                                                             $"Could not initialize configurer for {typeof(TService)} because null was passed to the ctor");

        internal static StandardConfigurer<ITransport> New(Injectionist injectionist) => new StandardConfigurer<ITransport>(injectionist);

        public static Injectionist Open(ToposProducerConfigurer configurer) => configurer._injectionist;

        public static Injectionist Open(ToposConsumerConfigurer configurer) => configurer._injectionist;
    }
}