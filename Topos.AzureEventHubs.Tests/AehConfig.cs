using System;
using System.IO;

namespace Topos.AzureEventHubs.Tests
{
    public static class AehConfig
    {
        static AehConfig()
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "azure_event_hubs_connection_string.secret.txt");

            ConnectionString = File.ReadAllText(filePath);
        }

        public static string ConnectionString { get; }
    }
}