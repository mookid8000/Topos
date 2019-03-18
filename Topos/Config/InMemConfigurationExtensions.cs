using Topos.Consumer;
using Topos.InMem;

namespace Topos.Config
{
    public static class InMemConfigurationExtensions
    {
        public static void UseInMemory(this StandardConfigurer<IProducerImplementation> configurer, InMemEventBroker eventBroker)
        {

        }

        public static void UseInMemory(this StandardConfigurer<IConsumerImplementation> configurer, InMemEventBroker eventBroker)
        {

        }

        public static void StoreInMemory(this StandardConfigurer<IPositionManager> configurer)
        {
            
        }
    }
}