using Topos.InMem;

namespace Topos.Config
{
    public static class InMemConfigurationExtensions
    {
        public static void UseInMemory(this StandardConfigurer<IToposProducerImplementation> configurer, InMemEventBroker eventBroker)
        {

        }

        public static void UseInMemory(this StandardConfigurer<IToposConsumerImplementation> configurer, InMemEventBroker eventBroker)
        {

        }
    }
}