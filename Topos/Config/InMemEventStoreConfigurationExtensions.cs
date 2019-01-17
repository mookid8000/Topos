using Topos.EventStore;
using Topos.EventStore.InMem;

namespace Topos.Config
{
    public static class InMemEventStoreConfigurationExtensions
    {
        public static void UseInMemory(this StandardConfigurer<IEventStore> configurer, InMemEventStore inMemEventStore)
        {
            StandardConfigurer.Open(configurer).Register(c => inMemEventStore);
        }
    }
}