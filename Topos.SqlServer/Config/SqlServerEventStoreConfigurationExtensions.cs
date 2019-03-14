using Topos.Config;

namespace Topos.SqlServer.Config
{
    public static class SqlServerEventStoreConfigurationExtensions
    {
        public static void UseSqlServer(this StandardConfigurer<IToposProducerImplementation> configurer, string connectionString)
        {
            
        }

        public static void UseSqlServer(this StandardConfigurer<IToposConsumerImplementation> configurer, string connectionString)
        {
            
        }
    }
}