using Topos.Config;

namespace Topos.Routing
{
    public class SubscriptionsRegistrar
    {
        readonly Topics _topics;

        public SubscriptionsRegistrar(Topics topics)
        {
            _topics = topics;
        }

        public SubscriptionsRegistrar Subscribe(string topic)
        {
            _topics.AddRange(new[] {topic});
            return this;
        }
    }
}