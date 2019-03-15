using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Topos.Config;
#pragma warning disable 1998

namespace Topos.Kafka.Tests
{
    [TestFixture]
    public class ReadmeCode : KafkaFixtureBase
    {
        [Test]
        public async Task SImpleCodeSample()
        {
            var count = 100000;

            await Time.Action(
                label: "sending",
                count: count,
                action: async () =>
                {
                    var producer = Configure
                        .Producer(c => c.UseKafka("localhost:9092"))
                        .Serialization(s => s.UseNewtonsoftJson())
                        .Topics(m => m.Map<SomeEvent>("someevents"))
                        .Create();

                    var messages = Enumerable.Range(0, count)
                        .Select(n => new SomeEvent($"This is event number {n}"));

                    await Task.WhenAll(messages.Select(m => producer.Send(m)));
                });
        }

        [Test]
        public async Task SimpleProducer()
        {
            var producer = Configure
                .Producer(c => c.UseKafka("localhost:9092"))
                .Serialization(s => s.UseNewtonsoftJson())
                .Topics(m => m.Map<SomeEvent>("someevents"))
                .Create();

            // keep producer instance for the entire life of your app,
            // remembering to dispose it when we shut down
            Using(producer);

            // send events like this:;
            await producer.Send(new SomeEvent("This is just a message"), partitionKey: "customer-004");
        }

        [Test]
        public void SimpleConsumer()
        {
            var consumer = Configure
                .Consumer("default-group", c => c.UseKafka("localhost:9092"))
                .Serialization(s => s.UseNewtonsoftJson())
                .Subscribe("someevents")
                .Handle(async (messages, token) =>
                {
                    foreach (var message in messages)
                    {
                        switch (message.Body)
                        {
                            case SomeEvent someEvent:
                                Console.WriteLine($"Got some event: {someEvent}");
                                break;
                        }
                    }
                })
                .Start();

            // dispose consumer when you want to stop consuming messages
            Using(consumer);

            Thread.Sleep(TimeSpan.FromSeconds(10));
        }

        class SomeEvent
        {
            public string Text { get; }

            public SomeEvent(string text)
            {
                Text = text;
            }
        }
    }
}