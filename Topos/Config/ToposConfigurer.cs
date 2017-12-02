using System;
using Topos.Internals;
using Topos.Transport;

namespace Topos.Config
{
    public class ToposConfigurer
    {
        readonly Injectionist _injectionist = new Injectionist();

        public ToposConfigurer Transport(Action<StandardConfigurer<ITransport>> configure)
        {
            configure(StandardConfigurer<ITransport>.New(_injectionist));
            return this;
        }

        void RegisterDefaults()
        {
            throw new NotImplementedException();
        }

        public ITopos Start()
        {
            RegisterDefaults();

            var result = _injectionist.Get<ITopos>();

            return result.Instance;
        }
    }
}