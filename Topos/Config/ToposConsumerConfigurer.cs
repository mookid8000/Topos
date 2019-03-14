using System;
using Topos.Internals;

namespace Topos.Config
{
    public class ToposConsumerConfigurer 
    {
        internal readonly Injectionist _injectionist = new Injectionist();

        public ToposConsumerConfigurer(Action<StandardConfigurer<IToposConsumerImplementation>> configure)
        {
            var configurer = StandardConfigurer<IToposConsumerImplementation>.New(_injectionist);

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