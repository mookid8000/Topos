using Topos.EventStore;

namespace Topos.Config
{
    public static class AzureEventHubsConfigurationExtensions
    {
        public static void UseAzureEventHubs(this StandardConfigurer<IEventStore> configuirer, string connectionString)
        {

        }
    }
}
