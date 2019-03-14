using System;
using NUnit.Framework;
using Topos.Consumer;

namespace Topos.Tests
{
    [TestFixture]
    public class TestPosition
    {
        [Test]
        public void CanFormatPartitionNice()
        {
            var position = new Position("default", 43, 478397389742L);

            Console.WriteLine($"Position: {position}");
        }
    }
}