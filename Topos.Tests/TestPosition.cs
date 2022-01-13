using System;
using NUnit.Framework;
using Serilog;
using Topos.Consumer;

namespace Topos.Tests;

[TestFixture]
public class TestPosition
{
    [Test]
    public void CanFormatPartitionNice()
    {
        Log.Information("HEJ");

        var position = new Position("default", 43, 478397389742L);

        Console.WriteLine($"Position: {position}");
    }
}