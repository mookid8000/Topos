using Topos.Config;
using Topos.Transport;

namespace Topos.SqlServer.Config
{
    public static class SqlServerTransportConfigurationExtensions
    {
        public static void UseSqlServer(this StandardConfigurer<ITransport> configurer, string connectionString)
        {
            
        }
    }
}