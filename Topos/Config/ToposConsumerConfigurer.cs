using System;
using Topos.Internals;

namespace Topos.Config
{
    public class ToposConsumerConfigurer 
    {
        internal readonly Injectionist _injectionist = new Injectionist();

        public ToposConsumerConfigurer(Action<StandardConfigurer<IToposConsumer>> configure)
        {
            var configurer = StandardConfigurer<IToposConsumer>.New(_injectionist);

            configure(configurer);
        }

        public IDisposable Start()
        {
            var consumer = this.Create();
            consumer.Start();
            return consumer;
        }
    }
}