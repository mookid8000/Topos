using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NUnit.Framework;
using Topos.Config;
using Topos.Producer;
using Topos.Tests.Extensions;
#pragma warning disable 1998

namespace Topos.Kafka.Tests
{
    [TestFixture]
    public class ProdConCatchUpTest : KafkaFixtureBase
    {
        string _topic;
        IToposProducer _producer;

        protected override void SetUp()
        {
            _topic = GetNewTopic();

            _producer = Configure
                .Producer(c => c.UseKafka(KafkaTestConfig.Address))
                .Logging(l => l.UseSerilog())
                .Topics(m => m.Map<string>(_topic))
                .Create();

            Using(_producer);
        }

        [Test]
        public async Task ThuisMustWork()
        {
            var receivedEvents = new ConcurrentQueue<string>();

            string FormatReceivedEvents() => $@"Received events contains this:

{string.Join(Environment.NewLine, receivedEvents.Select(e => $"    {e}"))}";


            var partitionKey = "test";

            // send three mewsages
            await _producer.Send("HEJ", partitionKey: partitionKey);
            await _producer.Send("MED", partitionKey: partitionKey);
            await _producer.Send("DIG", partitionKey: partitionKey);

            // wait until they're received
            await ConsumeForSomeTime(receivedEvents, c => c.Count == 3, FormatReceivedEvents);

            Assert.That(receivedEvents, Is.EqualTo(new[] { "HEJ", "MED", "DIG" }), FormatReceivedEvents);

            // now clear the events and send 5 additional events
            receivedEvents.Clear();

            await _producer.Send("HEJ", partitionKey: partitionKey);
            await _producer.Send("IGEN", partitionKey: partitionKey);
            await _producer.Send("IGEN", partitionKey: partitionKey);
            await _producer.Send("OG", partitionKey: partitionKey);
            await _producer.Send("SÅ IGEN", partitionKey: partitionKey);

            // ... and then wait for them to arrive
            await ConsumeForSomeTime(receivedEvents, c => c.Count == 5, FormatReceivedEvents);

            Assert.That(receivedEvents.Count, Is.EqualTo(5), FormatReceivedEvents);
            Assert.That(receivedEvents, Is.EqualTo(new[] { "HEJ", "IGEN", "IGEN", "OG", "SÅ IGEN" }), FormatReceivedEvents);
        }

        async Task ConsumeForSomeTime(ConcurrentQueue<string> receivedEvents, Expression<Func<ConcurrentQueue<string>, bool>> completionExpression, Func<string> errorDetailsFactory)
        {
            var consumer = Configure
                .Consumer("default-group", c => c.UseKafka(KafkaTestConfig.Address))
                .Logging(l => l.UseSerilog())
                .Subscribe(_topic)
                .Handle(async (messages, cancellationToken) =>
                {
                    var strings = messages.Select(m => m.Body).OfType<string>().ToList();

                    Console.WriteLine($"Received these strings: {string.Join(", ", strings)}");

                    foreach (var str in strings)
                    {
                        receivedEvents.Enqueue(str);
                    }
                })
                .Start();

            using (consumer)
            {
                try
                {
                    await receivedEvents.WaitOrDie(completionExpression, timeoutSeconds: 10);
                }
                catch (TimeoutException exception)
                {
                    throw new TimeoutException($"Failed with details: {errorDetailsFactory()}", exception);
                }

                await Task.Delay(TimeSpan.FromSeconds(1));

                var compiledExpression = completionExpression.Compile();

                Assert.That(compiledExpression(receivedEvents), Is.True, $@"The expression

    {compiledExpression}

was no longer true! Got these events:

{string.Join(Environment.NewLine, receivedEvents.Select(e => $"    {e}"))}
");
            }
        }
    }
}