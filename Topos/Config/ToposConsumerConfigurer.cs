using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Topos.Consumer;
using Topos.Internals;
using Topos.Logging;
using Topos.Routing;
using Topos.Serialization;
// ReSharper disable ArgumentsStyleStringLiteral

namespace Topos.Config
{
    public class ToposConsumerConfigurer
    {
        internal readonly Injectionist _injectionist = new Injectionist();

        readonly Topics _topics = new Topics();
        readonly Handlers _handlers = new Handlers();
        readonly Options _options = new Options();

        public ToposConsumerConfigurer(Action<StandardConfigurer<IConsumerImplementation>> configure, string groupName)
        {
            var configurer = StandardConfigurer<IConsumerImplementation>.New(_injectionist);

            _injectionist.Register(c => new GroupId(groupName));

            configure(configurer);
        }

        public ToposConsumerConfigurer Positions(Action<StandardConfigurer<IPositionManager>> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            var configurer = StandardConfigurer<IPositionManager>.New(_injectionist);

            configure(configurer);

            return this;
        }

        public ToposConsumerConfigurer Topics(Action<SubscriptionsRegistrar> topicRegistrarCallback)
        {
            if (topicRegistrarCallback == null) throw new ArgumentNullException(nameof(topicRegistrarCallback));

            if (!_injectionist.Has<Topics>())
            {
                _injectionist.Register(c => _topics);
            }

            topicRegistrarCallback(new SubscriptionsRegistrar(_topics));

            return this;
        }

        public ToposConsumerConfigurer Options(Action<OptionsConfigurer> optionsConfigurerCallback)
        {
            optionsConfigurerCallback(new OptionsConfigurer(_options));
            return this;
        }

        public ToposConsumerConfigurer Handle(Func<IReadOnlyCollection<ReceivedLogicalMessage>, ConsumerContext, CancellationToken, Task> messageHandler)
        {
            if (!_injectionist.Has<Handlers>())
            {
                _injectionist.Register(c => _handlers);
            }

            _handlers.Add(new MessageHandler(messageHandler, _options));

            return this;
        }

        public IToposConsumer Create()
        {
            ToposConfigurerHelpers.RegisterCommonServices(_injectionist);

            _injectionist.PossiblyRegisterDefault(c => new ConsumerContext());

            _injectionist.PossiblyRegisterDefault<IConsumerDispatcher>(c =>
            {
                var loggerFactory = c.Get<ILoggerFactory>();
                var messageSerializer = c.Get<IMessageSerializer>();
                var handlers = c.Get<Handlers>(errorMessage: @"Failing to get the handlers is a sign that the consumer has not had any handlers configured.

Please remember to configure at least one handler by invoking the .Handle(..) configurer like this:

    Configure.Consumer(...)
        .(...)
        .Handle(async (messages, context, cancellationToken) =>
        {
            // handle messages
        })
        .Start()
");
                var positionManager = c.Get<IPositionManager>(errorMessage: @"The consumer dispatcher needs access to a positions manager, so it can store a 'low water mark' position for each topic/partition.

It can be configured by invoking the .Positions(..) configurer like this:

    Configure.Consumer(...)
        .(...)
        .Positions(p => p.StoreIn(...))
        .Start()

");

                var consumerContext = c.Get<ConsumerContext>();

                return new DefaultConsumerDispatcher(loggerFactory, messageSerializer, handlers, positionManager, consumerContext);
            });

            _injectionist.Register<IToposConsumer>(c =>
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

            var resolutionResult = _injectionist.Get<IToposConsumer>();

            foreach (var initializable in resolutionResult.TrackedInstances.OfType<IInitializable>())
            {
                initializable.Initialize();
            }

            return resolutionResult.Instance;
        }

        public IDisposable Start()
        {
            var consumer = Create();
            consumer.Start();
            return consumer;
        }
    }
}