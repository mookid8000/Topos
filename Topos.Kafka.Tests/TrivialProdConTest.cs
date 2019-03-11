using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Topos.EventProcessing;
using Topos.Serilog;
using Topos.Tests.Extensions;
#pragma warning disable 1998

namespace Topos.Kafka.Tests
{
    [TestFixture]
    public class TrivialProdConTest : KafkaFixtureBase
    {
        string _topic;

        protected override void SetUp()
        {
            _topic = GetNewTopic();
        }

        [Test]
        public async Task CanDoIt()
        {
            var producer = CreateProducer();

            await producer.SendAsync(_topic, new[]
            {
                new KafkaEvent("key1", "hej"),
                new KafkaEvent("key1", "med"),
                new KafkaEvent("key1", "dig"),
                new KafkaEvent("key1", "min"),
                new KafkaEvent("key1", "ven"),
            });

            var receivedEvents = new ConcurrentQueue<string>();

            StartConsumer(async (evt, pos, token) => receivedEvents.Enqueue(evt.Body));

            await receivedEvents.WaitOrDie(q => q.Count == 5, timeoutSeconds: 10);

            Assert.That(receivedEvents, Is.EqualTo(new[] { "hej", "med", "dig", "min", "ven" }));
        }

        KafkaProducer CreateProducer()
        {
            var producer = new KafkaProducer(
                loggerFactory: new SerilogLoggerFactory(),
                address: KafkaTestConfig.Address
            );

            Using(producer);

            return producer;
        }


        void StartConsumer(Func<KafkaEvent, Position, CancellationToken, Task> eventHandler, string group = "default-group")
        {
            var consumer = new KafkaConsumer(
                loggerFactory: new SerilogLoggerFactory(),
                address: KafkaTestConfig.Address,
                topics: new[] { _topic },
                group: group,
                eventHandler: eventHandler
            );

            Using(consumer);

            consumer.Start();
        }
    }
}