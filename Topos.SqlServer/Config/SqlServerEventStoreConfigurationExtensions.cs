using Topos.Config;

namespace Topos.SqlServer.Config;

public static class SqlServerEventStoreConfigurationExtensions
{
    public static void UseSqlServer(this StandardConfigurer<IProducerImplementation> configurer, string connectionString)
    {
            
    }

    public static void UseSqlServer(this StandardConfigurer<IConsumerImplementation> configurer, string connectionString)
    {
            
    }
}