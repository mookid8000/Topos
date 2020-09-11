using System;
using System.Linq;
using Topos.Consumer;
using Topos.Internals;
using Topos.Logging;
using Topos.Producer;
using Topos.Serialization;
// ReSharper disable ArgumentsStyleStringLiteral

namespace Topos.Config
{
    public class ToposProducerConfigurer
    {
        internal readonly Injectionist _injectionist = new Injectionist();

        public ToposProducerConfigurer(Action<StandardConfigurer<IProducerImplementation>> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            var configurer = StandardConfigurer<IProducerImplementation>.New(_injectionist);

            configure(configurer);
        }

        public IToposProducer Create()
        {
            ToposConfigurerHelpers.RegisterCommonServices(_injectionist);

            _injectionist.Register<IToposProducer>(c =>
            {
                var messageSerializer = c.Get<IMessageSerializer>();
                var producerImplementation = c.Get<IProducerImplementation>(errorMessage: "Failing to get the producer implementation can be caused by a missing registration of IProducerImplementation");
                var loggerFactory = c.Get<ILoggerFactory>();

                var defaultToposProducer = new DefaultToposProducer(
                    messageSerializer,
                    producerImplementation,
                    loggerFactory
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

            var resolutionResult = _injectionist.Get<IToposProducer>();

            foreach (var initializable in resolutionResult.TrackedInstances.OfType<IInitializable>())
            {
                initializable.Initialize();
            }

            return resolutionResult.Instance;
        }
    }
}