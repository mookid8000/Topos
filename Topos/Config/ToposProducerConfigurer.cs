using System;
using System.Linq;
using Topos.Internals;
using Topos.Logging;
using Topos.Producer;
using Topos.Routing;
using Topos.Serialization;
// ReSharper disable ArgumentsStyleStringLiteral

namespace Topos.Config
{
    public class ToposProducerConfigurer
    {
        internal readonly Injectionist _injectionist = new Injectionist();

        readonly TopicMappings _topicMappings = new TopicMappings();

        public ToposProducerConfigurer(Action<StandardConfigurer<IProducerImplementation>> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            var configurer = StandardConfigurer<IProducerImplementation>.New(_injectionist);

            configure(configurer);
        }

        public ToposProducerConfigurer Topics(Action<TopicMapper> mapper)
        {
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            if (!_injectionist.Has<ITopicMapper>())
            {
                _injectionist.Register(c => _topicMappings.BuildTopicMapper());
            }

            mapper(new TopicMapper(_topicMappings));
            return this;
        }

        public IToposProducer Create()
        {
            ToposConfigurerHelpers.RegisterCommonServices(_injectionist);

            _injectionist.PossiblyRegisterDefault<ITopicMapper>(c => new SimpleTopicMapper());

            _injectionist.Register<IToposProducer>(c =>
            {
                var messageSerializer = c.Get<IMessageSerializer>();
                var topicMapper = c.Get<ITopicMapper>();
                var producerImplementation = c.Get<IProducerImplementation>(errorMessage: "Failing to get the producer implementation can be caused by a missing registration of IProducerImplementation");
                var loggerFactory = c.Get<ILoggerFactory>();

                var defaultToposProducer = new DefaultToposProducer(
                    messageSerializer,
                    topicMapper,
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

            return resolutionResult.Instance;
        }
    }
}