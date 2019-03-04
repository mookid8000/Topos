using Topos.Broker;
using Topos.Broker.InMem;

namespace Topos.Config
{
    public static class InMemEventStoreConfigurationExtensions
    {
        public static void UseInMemory(this StandardConfigurer<IEventBroker> configurer, InMemEventBroker inMemEventBroker)
        {
            StandardConfigurer.Open(configurer).Register(c => inMemEventBroker);
        }
    }
}