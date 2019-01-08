using System;
using Topos.Internals;

namespace Topos.Config
{
    public class ToposConsumerConfigurer 
    {
        internal readonly Injectionist Injectionist = new Injectionist();

        public IDisposable Start() => this.Create().Start();
    }
}