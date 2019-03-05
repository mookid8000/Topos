
namespace Topos.Config
{
    public static class AzureEventHubsConfigurationExtensions
    {
        public static void UseAzureEventHubs(this StandardConfigurer<IToposProducer> configuirer, string connectionString)
        {

        }

        public static void UseAzureEventHubs(this StandardConfigurer<IToposConsumer> configuirer, string connectionString)
        {

        }
    }
}
