using System.Text;
using Newtonsoft.Json;
using Topos.Serialization;
using Topos.Tests.Contracts.Factories;

namespace Topos.NewtonsoftJson.Tests;

public class NewtonsoftJsonMessageSerializerFactory : IMessageSeralizerFactory
{
    public IMessageSerializer Create() => new JsonSerializer(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }, Encoding.UTF8);
}