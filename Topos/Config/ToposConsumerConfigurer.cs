using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Topos.Consumer;
using Topos.Internals;
using Topos.Serialization;

namespace Topos.Config
{
    public class ToposConsumerConfigurer 
    {
        internal readonly Injectionist _injectionist = new Injectionist();

        readonly Topics _topics = new Topics();
        readonly Handlers _handlers = new Handlers();

        public ToposConsumerConfigurer(Action<StandardConfigurer<IConsumerImplementation>> configure)
        {
            var configurer = StandardConfigurer<IConsumerImplementation>.New(_injectionist);

            configure(configurer);
        }

        public ToposConsumerConfigurer Subscribe(params string[] topics)
        {
            if (!_injectionist.Has<Topics>())
            {
                _injectionist.Register(c => _topics);
            }

            _topics.AddRange(topics);

            return this;
        }

        public ToposConsumerConfigurer Handle(Func<IReadOnlyCollection<LogicalMessage>, CancellationToken, Task> messageHandler)
        {
            if (!_injectionist.Has<Handlers>())
            {
                _injectionist.Register(c => _handlers);
            }

            _handlers.Add(new MessageHandler(messageHandler));

            return this;
        }

        public IDisposable Start()
        {
            var consumer = this.Create();
            consumer.Start();
            return consumer;
        }
    }
}