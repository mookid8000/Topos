using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Topos.Consumer;

namespace Topos.Tests.Consumer;

[TestFixture]
public class TestConsumerContext
{
    [Test]
    public void CanRoundtripSomeStuff_String()
    {
        var context = new ConsumerContext();

        context.SetItem("hej");

        Assert.That(context.GetItem<string>(), Is.EqualTo("hej"));
    }

    [Test]
    public void CanRoundtripSomeStuff_StringAndHashSet()
    {
        var context = new ConsumerContext();

        var now = DateTimeOffset.Now;

        context.SetItem("hej");
        context.SetItem(new HashSet<DateTimeOffset> { now });

        Assert.That(context.GetItem<string>(), Is.EqualTo("hej"));
        Assert.That(context.GetItem<HashSet<DateTimeOffset>>().ToArray(), Is.EqualTo(new[] { now }));
    }
}