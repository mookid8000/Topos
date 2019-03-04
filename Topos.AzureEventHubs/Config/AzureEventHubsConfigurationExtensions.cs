using Topos.Broker;

namespace Topos.Config
{
    public static class AzureEventHubsConfigurationExtensions
    {
        public static void UseAzureEventHubs(this StandardConfigurer<IEventBroker> configuirer, string connectionString)
        {

        }
    }
}
