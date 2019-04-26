using System;
using System.Linq;
using NUnit.Framework;
using Tababular;
using Topos.Consumer;
using Topos.Internals;

namespace Topos.Kafkaesque.Tests
{
    [TestFixture]
    public class TestPositionExtensions
    {
        [Test]
        public void CanDoIt_Default()
        {
            var position = Position.Default("bimse", 0);
            var defaultForRandomTopic = position.ToKafkaesquePosition();

            Assert.That(defaultForRandomTopic.FileNumber, Is.EqualTo(-1));
            Assert.That(defaultForRandomTopic.BytePosition, Is.EqualTo(-1));
        }

        [Test]
        public void CanDoIt_Initial()
        {
            var position = new Position("bimse", 0, 0);
            var defaultForRandomTopic = position.ToKafkaesquePosition();

            Assert.That(defaultForRandomTopic.FileNumber, Is.EqualTo(0));
            Assert.That(defaultForRandomTopic.BytePosition, Is.EqualTo(0));
        }

        [Test]
        public void PrintExamples()
        {
            var formatter = new TableFormatter(new Hints { CollapseVerticallyWhenSingleLine = true });

            var positions = Enumerable.Range(0, 10)
                .SelectMany(fileNumber => Enumerable.Range(0, 100).Select(bytePosition => new{fileNumber, bytePosition}))
                .Select(a => new KafkaesquePosition(a.fileNumber, a.bytePosition))
                .Select(p =>
                {
                    var position = p.ToPosition("topic", 0);
                    var roundtrippedKafkaesquePosition = position.ToKafkaesquePosition();

                    return new
                    {
                        KafkaesquePosition = p,
                        Position = position,
                        RoundtrippedKafkaesquePosition = roundtrippedKafkaesquePosition
                    };
                });

            Console.WriteLine(formatter.FormatObjects(positions));
        }

        [Test]
        public void CanDoIt_Random()
        {
            var random = new Random(DateTime.Now.GetHashCode());

            long GetRandomNumber()
            {
                var fileNumber = (uint)random.Next(int.MaxValue);
                var bytePosition = (uint)random.Next(int.MaxValue);
                return (long)(((ulong)fileNumber << 32) | bytePosition);
            }

            var randomNumbers = Enumerable.Range(0, 1000).Select(_ => GetRandomNumber());

            foreach (var number in randomNumbers)
            {
                var position = new Position("whatever", 0, number);
                var kafkaesquePosition = position.ToKafkaesquePosition();
                var roundtrippedPosition = kafkaesquePosition.ToPosition("whatever", 0);

                Assert.That(roundtrippedPosition, Is.EqualTo(position), $@"

        Random number: {number}

     Initial position: {position}

  Kafkaesque position: {kafkaesquePosition}

Roundtripped position: {roundtrippedPosition}
");
            }
        }
    }
}