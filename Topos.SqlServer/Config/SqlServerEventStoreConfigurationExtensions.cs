using Topos.Config;

namespace Topos.SqlServer.Config
{
    public static class SqlServerEventStoreConfigurationExtensions
    {
        public static void UseSqlServer(this StandardConfigurer<IToposProducer> configurer, string connectionString)
        {
            
        }

        public static void UseSqlServer(this StandardConfigurer<IToposConsumer> configurer, string connectionString)
        {
            
        }
    }
}