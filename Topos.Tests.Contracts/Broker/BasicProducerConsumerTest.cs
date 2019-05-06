using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Topos.Config;
using Topos.InMem;
using Topos.Tests.Contracts.Extensions;

// ReSharper disable ArgumentsStyleAnonymousFunction

#pragma warning disable 1998

namespace Topos.Tests.Contracts.Broker
{
    public abstract class BasicProducerConsumerTest<TProducerFactory> : ToposContractFixtureBase where TProducerFactory : IBrokerFactory, new()
    {
        IBrokerFactory _brokerFactory;

        [Test]
        public async Task CanOvercomeExceptions()
        {
            var receivedStrings = new ConcurrentQueue<string>();
            var topic = BrokerFactory.GetNewTopic();

            var producer = BrokerFactory.ConfigureProducer()
                .Topics(m => m.Map<string>(topic))
                .Create();

            Using(producer);

            var random = new Random(DateTime.Now.GetHashCode());

            var consumer = BrokerFactory.ConfigureConsumer("default-group")
                .Handle(async (messages, token) =>
                {
                    var strings = messages.Select(m => m.Body).Cast<string>();

                    if (random.Next(3) == 0) throw new ApplicationException("oh no!");

                    receivedStrings.Enqueue(strings);
                })
                .Topics(t => t.Subscribe(topic))
                .Positions(p => p.StoreInMemory())
                .Start();

            Using(consumer);

            await Task.WhenAll(Enumerable.Range(0, 1000).Select(n => producer.Send($"message-{n}", "p100")));

            await receivedStrings.WaitOrDie(c => c.Count == 1000, failExpression: c => c.Count > 1000, timeoutSeconds: 10);
        }

        [Test]
        public async Task ConsumerCanPickUpWhereItLeftOff()
        {
            var receivedStrings = new ConcurrentQueue<string>();
            var topic = BrokerFactory.GetNewTopic();

            var producer = BrokerFactory.ConfigureProducer()
                .Topics(m => m.Map<string>(topic))
                .Create();

            Using(producer);

            IDisposable CreateConsumer(InMemPositionsStorage storage)
            {
                return BrokerFactory.ConfigureConsumer("default-group")
                    .Handle(async (messages, token) =>
                    {
                        var strings = messages.Select(m => m.Body).Cast<string>();

                        receivedStrings.Enqueue(strings);
                    })
                    .Topics(t => t.Subscribe(topic))
                    .Positions(p => p.StoreInMemory(storage))
                    .Start();
            }

            var positionsStorage = new InMemPositionsStorage();

            const string partitionKey = "same-every-time";

            using (CreateConsumer(positionsStorage))
            {
                await producer.Send("HEJ", partitionKey: partitionKey);
                await producer.Send("MED", partitionKey: partitionKey);
                await producer.Send("DIG", partitionKey: partitionKey);

                string GetFailureDetailsFunction() => $@"Got these strings:

{receivedStrings.ToPrettyJson()}";

                await receivedStrings.WaitOrDie(
                    completionExpression: q => q.Count == 3,
                    failExpression: q => q.Count > 3,
                    failureDetailsFunction: GetFailureDetailsFunction
                );
            }

            Console.WriteLine($@"Got these positions after FIRST run:

{string.Join(Environment.NewLine, positionsStorage.GetAll(topic).Select(position => $"    {position}"))}

");

            using (CreateConsumer(positionsStorage))
            {
                await producer.Send("MIN", partitionKey: partitionKey);
                await producer.Send("SØDE", partitionKey: partitionKey);
                await producer.Send("VEN", partitionKey: partitionKey);

                await receivedStrings.WaitOrDie(q => q.Count == 6, failExpression: q => q.Count > 6);

                // additional delay to be absolutely sure that no additional messages arrive after this point
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            Console.WriteLine($@"Got these positions after SECOND run:

{string.Join(Environment.NewLine, positionsStorage.GetAll(topic).Select(position => $"    {position}"))}

");

            Assert.That(receivedStrings.Count, Is.EqualTo(6), $@"Queue did not contain 6 strings as expected:

{receivedStrings.ToPrettyJson()}");

            var expectedReceivedStrings = new[]
            {
                "HEJ",
                "MED",
                "DIG",
                "MIN",
                "SØDE",
                "VEN",
            };

            Assert.That(receivedStrings, Is.EqualTo(expectedReceivedStrings), $@"

Expected

    {string.Join(", ", expectedReceivedStrings)}

but got

    {string.Join(", ", receivedStrings)}

");
        }

        [Test]
        public async Task CanStartProducer()
        {
            var producer = BrokerFactory.ConfigureProducer().Create();

            Using(producer);

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        [Test]
        public async Task CanStartConsumer()
        {
            var consumer = BrokerFactory.ConfigureConsumer("default-group")
                .Handle(async (messages, token) =>
                {
                    Console.WriteLine($"Received {messages.Count} msgs");
                })
                .Positions(p => p.StoreInMemory())
                .Create();

            Using(consumer);

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        [Test]
        public async Task CanProduceAndConsume()
        {
            var topic = BrokerFactory.GetNewTopic();

            var producer = BrokerFactory.ConfigureProducer()
                .Topics(m => m.Map<string>(topic))
                .Create();

            Using(producer);

            var gotTheString = new ManualResetEvent(false);

            var consumer = BrokerFactory.ConfigureConsumer("default-group")
                .Handle(async (messages, token) =>
                {
                    var receivedString = messages.Select(m => m.Body).FirstOrDefault() as string;

                    if (receivedString == "HEJ MED DIG MIN VEN")
                    {
                        gotTheString.Set();
                        return;
                    }

                    throw new ArgumentException($@"Did not receive the expected string 'HEJ MED DIG MIN VEN':

{messages.ToPrettyJson()}");
                })
                .Topics(t => t.Subscribe(topic))
                .Positions(p => p.StoreInMemory())
                .Start();

            Using(consumer);

            await producer.Send("HEJ MED DIG MIN VEN");

            gotTheString.WaitOrDie(errorMessage: "Waited for the text 'HEJ MED DIG MIN VEN' to arrive in the consumer");
        }

        IBrokerFactory BrokerFactory
        {
            get
            {
                if (_brokerFactory != null) return _brokerFactory;
                _brokerFactory = new TProducerFactory();
                Using(_brokerFactory);
                return _brokerFactory;
            }
        }
    }
}