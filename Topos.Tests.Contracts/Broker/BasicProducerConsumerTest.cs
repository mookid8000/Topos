using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Topos.Config;
using Topos.InMem;
using Topos.Tests.Contracts.Extensions;
using Topos.Tests.Contracts.Factories;
using Testy.Extensions;
using Topos.Producer;
using Topos.Tests.Contracts.Stubs;
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ArgumentsStyleAnonymousFunction
#pragma warning disable 1998

namespace Topos.Tests.Contracts.Broker
{
    public abstract class BasicProducerConsumerTest<TBrokerFactory> : ToposContractFixtureBase where TBrokerFactory : IBrokerFactory, new()
    {
        IBrokerFactory _brokerFactory;

        [Test]
        public async Task CatchUpTest_AllHistory()
        {
            var receivedMessages = new ConcurrentQueue<string>();
            var topic = BrokerFactory.GetNewTopic();
            var producer = BrokerFactory.ConfigureProducer().Create();

            Using(producer);

            await producer.Send(topic, new ToposMessage("message 1"));

            var consumer = BrokerFactory.ConfigureConsumer("default-group")
                .Handle(async (messages, context, token) => receivedMessages.EnqueueRange(messages.Select(m => m.Body).OfType<string>()))
                .Topics(t => t.Subscribe(topic))
                .Positions(p =>
                {
                    p.StoreInMemory();
                    p.SetInitialPosition(StartFromPosition.Beginning);
                })
                .Start();

            Using(consumer);

            await producer.Send(topic, new ToposMessage("message 2"));

            await receivedMessages.WaitOrDie(
                completionExpression: q => q.Count == 2,
                failExpression: q => q.Count > 2,
                failureDetailsFunction: () => $"Expected to receive exactly 2 messages, but got this: {string.Join("; ", receivedMessages)}"
            );
        }

        [Test]
        public async Task CatchUpTest_OnlyNew()
        {
            var receivedMessages = new ConcurrentQueue<string>();
            var topic = BrokerFactory.GetNewTopic();
            var producer = BrokerFactory.ConfigureProducer().Create();

            Using(producer);

            await producer.Send(topic, new ToposMessage("message 1"));

            var consumer = BrokerFactory.ConfigureConsumer("default-group")
                .Handle(async (messages, context, token) => receivedMessages.EnqueueRange(messages.Select(m => m.Body).OfType<string>()))
                .Topics(t => t.Subscribe(topic))
                .Positions(p =>
                {
                    p.StoreInMemory();
                    p.SetInitialPosition(StartFromPosition.Now);
                })
                .Start();

            Using(consumer);

            await producer.Send(topic, new ToposMessage("message 2"));

            await receivedMessages.WaitOrDie(
                completionExpression: q => q.Count == 1,
                failExpression: q => q.Count > 1,
                failureDetailsFunction: () => $"Expected to receive exactly 1 messages, but got this: {string.Join("; ", receivedMessages)}"
            );
        }

        [Test]
        public async Task DoesNotLogTaskCancelledException()
        {
            var topic = BrokerFactory.GetNewTopic();
            var producer = BrokerFactory.ConfigureProducer().Create();

            Using(producer);

            var logs = new ListLoggerFactory();
            var weAreInTheHandler = new ManualResetEvent(initialState: false);

            var consumer = BrokerFactory.ConfigureConsumer("default-group")
                .Logging(l => l.Use(logs))
                .Handle(async (messages, context, token) =>
                {
                    weAreInTheHandler.Set();
                    await Task.Delay(TimeSpan.FromSeconds(100), token);
                })
                .Topics(t => t.Subscribe(topic))
                .Positions(p => p.StoreInMemory())
                .Start();

            Using(consumer);

            await producer.Send(topic, new ToposMessage("wazzup my mayn?!"));

            weAreInTheHandler.WaitOrDie(timeoutSeconds: 15);

            // force everything to shut down now
            CleanUpDisposables();

            logs.DumpLogs();

            // check that we didn't get that silly TaskCancelledException in the logs
            var logLineWithException = logs.FirstOrDefault(l => l.Exception != null);

            Assert.That(logLineWithException, Is.Null,
                $"Didn't expect any exceptions, but we got this: {logLineWithException?.Exception}");
        }

        [Test]
        public async Task CanOvercomeExceptions()
        {
            var receivedStrings = new ConcurrentQueue<string>();
            var topic = BrokerFactory.GetNewTopic();

            var producer = BrokerFactory.ConfigureProducer().Create();

            Using(producer);

            var random = new Random(DateTime.Now.GetHashCode());

            var consumer = BrokerFactory.ConfigureConsumer("default-group")
                .Handle(async (messages, context, token) =>
                {
                    var strings = messages.Select(m => m.Body).Cast<string>();

                    if (random.Next(3) == 0) throw new ApplicationException("oh no!");

                    receivedStrings.EnqueueRange(strings);
                })
                .Topics(t => t.Subscribe(topic))
                .Positions(p => p.StoreInMemory())
                .Start();

            Using(consumer);

            await Task.WhenAll(Enumerable.Range(0, 1000).Select(n => producer.Send(topic, new ToposMessage($"message-{n}"), "p100")));

            await receivedStrings.WaitOrDie(c => c.Count == 1000, failExpression: c => c.Count > 1000, timeoutSeconds: 20);
        }

        [Test]
        public async Task ConsumerCanPickUpWhereItLeftOff()
        {
            var receivedStrings = new ConcurrentQueue<string>();
            var topic = BrokerFactory.GetNewTopic();

            var producer = BrokerFactory.ConfigureProducer().Create();

            Using(producer);

            IDisposable CreateConsumer(InMemPositionsStorage storage)
            {
                return BrokerFactory.ConfigureConsumer("default-group")
                    .Handle(async (messages, context, token) =>
                    {
                        var strings = messages.Select(m => m.Body).Cast<string>();

                        receivedStrings.EnqueueRange(strings);
                    })
                    .Topics(t => t.Subscribe(topic))
                    .Positions(p => p.StoreInMemory(storage))
                    .Start();
            }

            var positionsStorage = new InMemPositionsStorage();

            const string partitionKey = "same-every-time";

            string GetFailureDetailsFunction() => $@"Got these strings:

{receivedStrings.ToPrettyJson()}";

            using (CreateConsumer(positionsStorage))
            {
                await producer.Send(topic, new ToposMessage("HEJ"), partitionKey: partitionKey);
                await producer.Send(topic, new ToposMessage("MED"), partitionKey: partitionKey);
                await producer.Send(topic, new ToposMessage("DIG"), partitionKey: partitionKey);

                await receivedStrings.WaitOrDie(
                    completionExpression: q => q.Count == 3,
                    failExpression: q => q.Count > 3,
                    failureDetailsFunction: GetFailureDetailsFunction,
                    timeoutSeconds: 10
                );
            }

            Console.WriteLine($@"Got these positions after FIRST run:

{string.Join(Environment.NewLine, positionsStorage.GetAll(topic).Select(position => $"    {position}"))}

");

            using (CreateConsumer(positionsStorage))
            {
                await producer.Send(topic, new ToposMessage("MIN"), partitionKey: partitionKey);
                await producer.Send(topic, new ToposMessage("SØDE"), partitionKey: partitionKey);
                await producer.Send(topic, new ToposMessage("VEN"), partitionKey: partitionKey);

                await receivedStrings.WaitOrDie(
                    completionExpression: q => q.Count == 6,
                    failExpression: q => q.Count > 6,
                    timeoutSeconds: 20
                );

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
                .Handle(async (messages, context, token) =>
                {
                    Console.WriteLine($"Received {messages.Count} msgs");
                })
                .Positions(p => p.StoreInMemory())
                .Topics(t => t.Subscribe("topic1").Subscribe("topic2"))
                .Create();

            Using(consumer);

            consumer.Start();

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        [Test]
        public async Task CanProduceAndConsume()
        {
            var topic = BrokerFactory.GetNewTopic();

            var producer = BrokerFactory.ConfigureProducer().Create();

            Using(producer);

            var gotTheString = new ManualResetEvent(false);

            var consumer = BrokerFactory.ConfigureConsumer("default-group")
                .Handle(async (messages, context, token) =>
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

            await producer.Send(topic, new ToposMessage("HEJ MED DIG MIN VEN"));

            gotTheString.WaitOrDie(errorMessage: "Waited for the text 'HEJ MED DIG MIN VEN' to arrive in the consumer");
        }

        IBrokerFactory BrokerFactory
        {
            get
            {
                if (_brokerFactory != null) return _brokerFactory;
                _brokerFactory = new TBrokerFactory();
                Using(_brokerFactory);
                return _brokerFactory;
            }
        }
    }
}