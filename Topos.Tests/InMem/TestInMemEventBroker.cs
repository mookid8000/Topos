using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Topos.InMem;
using Topos.Serialization;

namespace Topos.Tests.InMem;

[TestFixture]
public class TestInMemEventBroker : ToposFixtureBase
{
    InMemEventBroker _broker;

    protected override void SetUp()
    {
        _broker = new InMemEventBroker();
    }

    [Test]
    public void CanReceiveTransportMessage_GetsNullWhenEmpty()
    {
        var randomTopics = Enumerable.Range(0, 10).Select(n => $"topic-{Guid.NewGuid()}");

        foreach (var topic in randomTopics)
        {
                
        }
    }

    [Test]
    public void CanPublishMessages()
    {
        _broker.Send("topic-a", new TransportMessage(new Dictionary<string, string>(), new byte[] {1, 2, 3}));
    }
}