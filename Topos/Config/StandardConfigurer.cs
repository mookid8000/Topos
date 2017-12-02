using Topos.Internals;
using Topos.Transport;

namespace Topos.Config
{
    public class StandardConfigurer<TService>
    {
        readonly Injectionist _injectionist;

        StandardConfigurer(Injectionist injectionist) => _injectionist = injectionist;

        internal static StandardConfigurer<ITransport> New(Injectionist injectionist) => new StandardConfigurer<ITransport>(injectionist);
    }
}