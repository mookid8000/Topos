using Topos.Internals;

namespace Topos.Config
{
    public class ToposConsumerConfigurer 
    {
        internal readonly Injectionist _injectionist = new Injectionist();

        public void Start() => this.Create().Start();
    }
}