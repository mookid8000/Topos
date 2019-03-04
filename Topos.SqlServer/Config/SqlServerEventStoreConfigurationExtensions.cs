using Topos.Broker;
using Topos.Config;

namespace Topos.SqlServer.Config
{
    public static class SqlServerEventStoreConfigurationExtensions
    {
        public static void UseSqlServer(this StandardConfigurer<IEventBroker> configurer, string connectionString)
        {
            
        }
    }
}