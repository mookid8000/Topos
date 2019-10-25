using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Testy.Benchmarking;
using Testy.Extensions;
using Topos.Config;
using Topos.Producer;
using Topos.Tests.Contracts.Factories;
// ReSharper disable ArgumentsStyleAnonymousFunction
#pragma warning disable 1998

namespace Topos.Tests.Contracts.Broker
{
    public abstract class BatchProducerTest<TBrokerFactory> : ToposContractFixtureBase where TBrokerFactory : IBrokerFactory, new()
    {
        TBrokerFactory _factory;

        protected override void AdditionalSetUp() => _factory = Using(new TBrokerFactory());

        [TestCase(10, true)]
        [TestCase(10, false)]
        public async Task ProduceEvents(int eventCount, bool useBatchApi)
        {
            var events = Enumerable.Range(0, eventCount)
                .Select(n => new ToposMessage($"THIS IS EVENT NUMBER {n}"));

            var topic = _factory.GetNewTopic();

            var producer = Using(
                _factory.ConfigureProducer()
                    .Topics(t => t.Map<string>(topic))
                    .Create()
            );

            using (new TimerScope($"Send {eventCount} messages", countForRateCalculation: eventCount))
            {
                if (!useBatchApi)
                {
                    foreach (var evt in events)
                    {
                        await producer.Send(evt, partitionKey: "whatever");
                    }
                }
                else
                {
                    await producer.SendMany(events, partitionKey: "whatever");
                }
            }

            var queue = new ConcurrentQueue<string>();

            var consumer = Using(
                _factory
                    .ConfigureConsumer("default")
                    .Topics(t => t.Subscribe(topic))
                    .Handle(async (messages, context, cancellationToken) =>
                    {
                        queue.EnqueueRange(messages.Select(m => m.Body).Cast<string>());
                    })
                    .Positions(p => p.StoreInMemory())
                    .Create()
            );

            consumer.Start();

            using (new TimerScope($"Receive {eventCount} messages", countForRateCalculation: eventCount))
            {
                await queue.WaitOrDie(q => q.Count == eventCount, failExpression: q => q.Count > eventCount);
            }
        }
    }
}