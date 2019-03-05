using Topos.InMem;

namespace Topos.Config
{
    public static class InMemConfigurationExtensions
    {
        public static void UseInMemory(this StandardConfigurer<IToposProducer> configurer, InMemEventBroker eventBroker)
        {

        }

        public static void UseInMemory(this StandardConfigurer<IToposConsumer> configurer, InMemEventBroker eventBroker)
        {

        }
    }
}