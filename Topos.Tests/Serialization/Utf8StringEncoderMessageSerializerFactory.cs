using Topos.Serialization;
using Topos.Tests.Contracts.Factories;

namespace Topos.Tests.Serialization;

public class Utf8StringEncoderMessageSerializerFactory : IMessageSeralizerFactory
{
    public IMessageSerializer Create() => new Utf8StringEncoder();
}