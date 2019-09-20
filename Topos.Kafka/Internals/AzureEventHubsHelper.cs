using System;
// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleStringLiteral

namespace Topos.Internals
{
    class AzureEventHubsHelper
    {
        public static void TrySetConnectionInfo(string bootstrapServers, Action<EventHubsConnectionInfo> applyInfo)
        {
            if (!LooksLikeEventHubs(bootstrapServers)) return;

            try
            {
                var parser = new ConnectionStringParser(bootstrapServers);

                var endpoint = parser.GetValue("Endpoint");
                var host = new Uri(endpoint).Host;

                applyInfo(new EventHubsConnectionInfo(bootstrapServers: $"{host}:9093", saslUsername: "$ConnectionString", saslPassword: bootstrapServers));
            }
            catch (Exception)
            {
                throw new ArgumentException("The connection string looks like an Azure Event Hubs connection string, but an error occurred when trying to parse it");
            }
        }

        static bool LooksLikeEventHubs(string bootstrapServers)
        {
            if (string.IsNullOrWhiteSpace(bootstrapServers)) return false;

            var parser = new ConnectionStringParser(bootstrapServers);

            return parser.HasElement("Endpoint")
                   && parser.HasElement("SharedAccessKeyName")
                   && parser.HasElement("SharedAccessKey");
        }

        public class EventHubsConnectionInfo
        {
            public string BootstrapServers { get; }
            public string SaslUsername { get; }
            public string SaslPassword { get; }

            public EventHubsConnectionInfo(string bootstrapServers, string saslUsername, string saslPassword)
            {
                BootstrapServers = bootstrapServers;
                SaslUsername = saslUsername;
                SaslPassword = saslPassword;
            }
        }
    }
}