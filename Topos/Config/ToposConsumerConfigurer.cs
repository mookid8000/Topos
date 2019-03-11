using System;
using Topos.Internals;

namespace Topos.Config
{
    public class ToposConsumerConfigurer 
    {
        internal readonly Injectionist _injectionist = new Injectionist();

        public IDisposable Start()
        {
            var consumer = this.Create();
            consumer.Start();
            return consumer;
        }
    }
}