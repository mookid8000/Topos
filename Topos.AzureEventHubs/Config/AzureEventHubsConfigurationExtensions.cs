
namespace Topos.Config
{
    public static class AzureEventHubsConfigurationExtensions
    {
        public static void UseAzureEventHubs(this StandardConfigurer<IProducerImplementation> configurer, string connectionString)
        {

        }

        public static void UseAzureEventHubs(this StandardConfigurer<IConsumerImplementation> configurer, string connectionString)
        {

        }
    }
}
