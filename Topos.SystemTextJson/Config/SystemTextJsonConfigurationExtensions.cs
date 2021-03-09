using System;
using System.Text.Json;
using Topos.Serialization;
using Topos.SystemTextJson;

namespace Topos.Config
{
    public static class SystemTextJsonConfigurationExtensions
    {
        /// <summary>
        /// Configures Topos to use .NET's built-in <see cref="JsonSerializer"/> to serialize/deserialize messages
        /// </summary>
        public static void UseSystemTextJson(this StandardConfigurer<IMessageSerializer> configurer)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));

            StandardConfigurer.Open(configurer).Register(_ => new SystemTextJsonSerializer());
        }
    }
}