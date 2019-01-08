using System;
using Topos.EventStore;
using Topos.Internals;

namespace Topos.Config
{
    public class StandardConfigurer<TService>
    {
        readonly Injectionist _injectionist;

        StandardConfigurer(Injectionist injectionist) => _injectionist = injectionist ?? throw new ArgumentNullException(nameof(injectionist), 
                                                                             $"Could not initialize configurer for {typeof(TService)} because null was passed to the ctor");

        internal static StandardConfigurer<IEventStore> New(Injectionist injectionist) => new StandardConfigurer<IEventStore>(injectionist);

        internal static Injectionist Open(ToposProducerConfigurer configurer) => configurer.Injectionist;

        internal static Injectionist Open(ToposConsumerConfigurer configurer) => configurer.Injectionist;

        internal static StandardConfigurer<IEventStore> New(ToposProducerConfigurer configurer)
        {
            var injectionist = Open(configurer);

            return New(injectionist);
        }

        internal static StandardConfigurer<IEventStore> New(ToposConsumerConfigurer configurer)
        {
            var injectionist = Open(configurer);

            return New(injectionist);
        }
    }
}