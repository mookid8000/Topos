using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using NUnit.Framework;
using Serilog.Events;
using Topos.Kafka;
using Topos.Logging;
using Topos.Tests.Extensions;
// ReSharper disable ArgumentsStyleAnonymousFunction
// ReSharper disable ArgumentsStyleStringLiteral
// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ArgumentsStyleNamedExpression
#pragma warning disable 1998

namespace Topos.Tests.Kafka
{
    [TestFixture]
    public class TestKafkaConsumer : MyFixtureBase
    {
        [Test]
        public async Task CanConsumeEvent()
        {
            var receivedEvents = new ConcurrentQueue<string>();

            var consumer = new KafkaConsumer(
                loggerFactory: new ConsoleLoggerFactory(),
                address: "localhost:9092",
                topics: new[] { "test-topic" },
                @group: "default3",
                eventHandler: async (evt, pos, token) => receivedEvents.Enqueue(evt.Body)
            );

            Using(consumer);

            consumer.Start();

            await receivedEvents.WaitOrDie(q => q.Count == 3, timeoutSeconds: 7);

            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        [Test]
        public async Task CanConsumeEvent_Lots()
        {
            SetLogLevelTo(LogEventLevel.Information);

            var receivedEvents = new ConcurrentQueue<string>();

            const string groupName = "default6";

            var consumer1 = new KafkaConsumer(
                loggerFactory: new ConsoleLoggerFactory(),
                address: "localhost:9092",
                topics: new[] { "lots" },
                @group: groupName,
                eventHandler: async (evt, pos, token) => receivedEvents.Enqueue(evt.Body)
            );

            Using(consumer1);

            var consumer2 = new KafkaConsumer(
                loggerFactory: new ConsoleLoggerFactory(),
                address: "localhost:9092",
                topics: new[] { "lots" },
                @group: groupName,
                eventHandler: async (evt, pos, token) => receivedEvents.Enqueue(evt.Body)
            );

            Using(consumer2);

            consumer1.Start();
            consumer2.Start();

            await receivedEvents.WaitOrDie(q => q.Count == 100000, timeoutSeconds: 70);

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}