using NUnit.Framework;
using Topos.Tests.Contracts.Serialization;

namespace Topos.NewtonsoftJson.Tests;

[TestFixture]
public class NewtonsoftJsonMessageSerializationTests : MessageSerializationTests<NewtonsoftJsonMessageSerializerFactory>
{
}