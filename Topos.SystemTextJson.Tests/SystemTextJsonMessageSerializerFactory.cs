using Topos.Serialization;
using Topos.Tests.Contracts.Factories;

namespace Topos.SystemTextJson.Tests;

public class SystemTextJsonMessageSerializerFactory : IMessageSeralizerFactory
{
    public IMessageSerializer Create() => new SystemTextJsonSerializer();
}