using System;
using Topos.Internals;
using Topos.Internals.Consumer;
using Topos.Internals.Producer;
using Topos.Logging;
using Topos.Logging.Console;

// ReSharper disable RedundantArgumentDefaultValue

namespace Topos.Config
{
    public static class ToposConfigurationFinalizer
    {
        public static IToposProducer Create(this ToposProducerConfigurer configurer)
        {
            var injectionist = StandardConfigurer<IToposProducer>.Open(configurer);

            RegisterCommonServices(injectionist);

            injectionist.Register<IToposProducer>(c => new ToposProducer(c.Get<ILoggerFactory>()));

            var resolutionResult = injectionist.Get<IToposProducer>();

            return resolutionResult.Instance;
        }

        public static IToposConsumer Create(this ToposConsumerConfigurer configurer)
        {
            var injectionist = StandardConfigurer<IToposConsumer>.Open(configurer);

            RegisterCommonServices(injectionist);

            injectionist.Register<IToposConsumer>(c => new ToposConsumer(c.Get<ILoggerFactory>()));

            var resolutionResult = injectionist.Get<IToposConsumer>();

            return resolutionResult.Instance;
        }

        static void RegisterCommonServices(Injectionist injectionist)
        {
            PossiblyRegisterDefault<ILoggerFactory>(injectionist, c => new ConsoleLoggerFactory());

        }

        static void PossiblyRegisterDefault<TService>(Injectionist injectionist, Func<IResolutionContext, TService> factory)
        {
            if (injectionist.Has<TService>(primary: true)) return;

            injectionist.Register(factory);
        }
    }
}