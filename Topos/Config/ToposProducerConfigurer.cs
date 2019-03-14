using System;
using Topos.Internals;

namespace Topos.Config
{
    public class ToposProducerConfigurer 
    {
        internal readonly Injectionist _injectionist = new Injectionist();

        public ToposProducerConfigurer(Action<StandardConfigurer<IToposProducer>> configure)
        {
            var configurer = StandardConfigurer<IToposProducer>.New(_injectionist);

            configure(configurer);
        }
    }
}