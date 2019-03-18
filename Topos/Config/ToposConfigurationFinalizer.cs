using System;
using System.Linq;
using Topos.Consumer;
using Topos.InMem;
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
                    foreach (var instance in c.TrackedInstances.OfType<IDisposable>().Reverse())
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

            injectionist.PossiblyRegisterDefault<IConsumerDispatcher>(c =>
            {
                var loggerFactory = c.Get<ILoggerFactory>();
                var messageSerializer = c.Get<IMessageSerializer>();
                var handlers = c.Get<Handlers>(errorMessage: @"Failing to get the handlers is a sign that the consumer has not had any handlers configured.

Please remember to configure at least one handler by invoking the .Handle(..) configurer like this:

    Configure.Consumer(...)
        .Handle(async (messages, cancellationToken) =>
        {
            // handle messages
        })
        .Start()
");
                var positionManager = c.Get<IPositionManager>();

                return new DefaultConsumerDispatcher(loggerFactory, messageSerializer, handlers, positionManager);
            });

            injectionist.PossiblyRegisterDefault<IPositionManager>(c => new InMemPositionsManager());

            injectionist.Register<IToposConsumer>(c =>
            {
                var toposConsumerImplementation = c.Get<IConsumerImplementation>();

                var defaultToposConsumer = new DefaultToposConsumer(toposConsumerImplementation);

                defaultToposConsumer.Disposing += () =>
                {
                    foreach (var instance in c.TrackedInstances.OfType<IDisposable>().Reverse())
                    {
                        instance.Dispose();
                    }
                };

                return defaultToposConsumer;
            });

            var resolutionResult = injectionist.Get<IToposConsumer>();

            foreach (var initializable in resolutionResult.TrackedInstances.OfType<IInitializable>())
            {
                initializable.Initialize();
            }

            return resolutionResult.Instance;
        }

        static void RegisterCommonServices(Injectionist injectionist)
        {
            injectionist.PossiblyRegisterDefault<ILoggerFactory>(c => new ConsoleLoggerFactory());
            injectionist.PossiblyRegisterDefault<IMessageSerializer>(c => new Utf8StringEncoder());
        }
    }
}