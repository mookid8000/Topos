
namespace Topos.Config
{
    public static class AzureEventHubsConfigurationExtensions
    {
        public static void UseAzureEventHubs(this StandardConfigurer<IToposProducerImplementation> configurer, string connectionString)
        {

        }

        public static void UseAzureEventHubs(this StandardConfigurer<IToposConsumerImplementation> configurer, string connectionString)
        {

        }
    }
}
