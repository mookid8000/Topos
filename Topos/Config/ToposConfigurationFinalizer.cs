using System;
using System.Linq;
using Topos.Consumer;
using Topos.Internals;
using Topos.Logging;
using Topos.Logging.Console;
using Topos.Producer;
using Topos.Routing;
using Topos.Serialization;
// ReSharper disable RedundantArgumentDefaultValue

namespace Topos.Config
{
    public static class ToposConfigurationFinalizer
    {
        public static IToposProducer Create(this ToposProducerConfigurer configurer)
        {
            var injectionist = StandardConfigurer.Open(configurer);

            RegisterCommonServices(injectionist);

            injectionist.Register<IToposProducer>(c =>
            {
                var messageSerializer = c.Get<IMessageSerializer>();
                var topicMapper = c.Get<ITopicMapper>();
                var producerImplementation = c.Get<IProducerImplementation>();

                var defaultToposProducer = new DefaultToposProducer(
                    messageSerializer,
                    topicMapper,
                    producerImplementation
                );

                defaultToposProducer.Disposing += () =>
                {
                    foreach (var instance in c.TrackedInstances.OfType<IDisposable>())
                    {
                        instance.Dispose();
                    }
                };

                return defaultToposProducer;
            });

            var resolutionResult = injectionist.Get<IToposProducer>();

            return resolutionResult.Instance;
        }

        public static IToposConsumer Create(this ToposConsumerConfigurer configurer)
        {
            var injectionist = StandardConfigurer.Open(configurer);

            RegisterCommonServices(injectionist);

            injectionist.Register<IToposConsumer>(c =>
            {
                var messageSerializer = c.Get<IMessageSerializer>();
                var toposConsumerImplementation = c.Get<IConsumerImplementation>();

                var defaultToposConsumer = new DefaultToposConsumer(
                    messageSerializer,
                    toposConsumerImplementation
                );

                defaultToposConsumer.Disposing += () =>
                {
                    foreach (var instance in c.TrackedInstances.OfType<IDisposable>())
                    {
                        instance.Dispose();
                    }
                };

                return defaultToposConsumer;
            });

            var resolutionResult = injectionist.Get<IToposConsumer>();

            return resolutionResult.Instance;
        }

        static void RegisterCommonServices(Injectionist injectionist)
        {
            PossiblyRegisterDefault<ILoggerFactory>(injectionist, c => new ConsoleLoggerFactory());
            PossiblyRegisterDefault<IMessageSerializer>(injectionist, c => new Utf8StringEncoder());
        }

        static void PossiblyRegisterDefault<TService>(Injectionist injectionist, Func<IResolutionContext, TService> factory)
        {
            if (injectionist.Has<TService>(primary: true)) return;

            injectionist.Register(factory);
        }
    }
}