using NUnit.Framework;
using Topos.Tests.Contracts.Serialization;

namespace Topos.Tests.Serialization;

[TestFixture]
public class Utf8StringEncoderMessageSerializationTests : MessageSerializationTests<Utf8StringEncoderMessageSerializerFactory>
{
}