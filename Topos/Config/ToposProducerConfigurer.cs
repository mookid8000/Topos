using System;
using Topos.Internals;

namespace Topos.Config
{
    public class ToposProducerConfigurer 
    {
        internal readonly Injectionist _injectionist = new Injectionist();

        public ToposProducerConfigurer(Action<StandardConfigurer<IToposProducerImplementation>> configure)
        {
            var configurer = StandardConfigurer<IToposProducerImplementation>.New(_injectionist);

            configure(configurer);
        }
    }
}