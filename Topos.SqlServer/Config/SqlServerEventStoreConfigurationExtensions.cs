using Topos.Config;
using Topos.EventStore;

namespace Topos.SqlServer.Config
{
    public static class SqlServerEventStoreConfigurationExtensions
    {
        public static void UseSqlServer(this StandardConfigurer<IEventStore> configurer, string connectionString)
        {
            
        }
    }
}