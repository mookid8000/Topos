using System.Collections.Generic;
using NUnit.Framework;
using Topos.Consumer;
using Topos.Extensions;
using Topos.Serialization;
using Topos.Tests.Contracts.Factories;

namespace Topos.Tests.Contracts.Serialization;

public abstract class MessageSerializationTests<TMessageSeralizerFactory> : ToposContractFixtureBase where TMessageSeralizerFactory : IMessageSeralizerFactory, new()
{
    IMessageSerializer _serializer;

    protected override void AdditionalSetUp() => _serializer = new TMessageSeralizerFactory().Create();

    [Test]
    public void CanRoundtripTHisBadBoy()
    {
        var headers = new Dictionary<string, string>
        {
            ["test-header"] = "test-value"
        };

        var logicalMessage = new LogicalMessage(headers, "hej med dig");
        var transportMessage = _serializer.Serialize(logicalMessage);
        var receivedTransportMessage = new ReceivedTransportMessage(Position.Default("random-topic", 0), transportMessage.Headers, transportMessage.Body);
        var receivedLogicalMessage = _serializer.Deserialize(receivedTransportMessage);

        Assert.That(receivedLogicalMessage.Headers.GetValue("test-header"), Is.EqualTo("test-value"));
    }
}