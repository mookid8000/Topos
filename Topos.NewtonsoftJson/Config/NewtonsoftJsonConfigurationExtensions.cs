using System;
using System.Text;
using Newtonsoft.Json;
using Topos.Serialization;
using JsonSerializer = Topos.NewtonsoftJson.JsonSerializer;

namespace Topos.Config;

public static class NewtonsoftJsonConfigurationExtensions
{
    public static void UseNewtonsoftJson(this StandardConfigurer<IMessageSerializer> configurer, JsonSerializerSettings settings = null, Encoding encoding = null)
    {
        if (configurer == null) throw new ArgumentNullException(nameof(configurer));

        StandardConfigurer.Open(configurer).Register(c => new JsonSerializer(settings: settings, encoding: encoding));
    }
}