using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Topos.Producer;
using Topos.Tests.Contracts.Factories;

namespace Topos.Tests.Contracts.Broker
{
    public abstract class BatchProducerTest<TBrokerFactory> : ToposContractFixtureBase where TBrokerFactory : IBrokerFactory, new()
    {
        TBrokerFactory _factory;

        protected override void AdditionalSetUp() => _factory = Using(new TBrokerFactory());

        [TestCase(10, true)]
        [TestCase(10, false)]
        [Ignore("not ready yet")]
        public async Task ProduceEvents(int eventCount, bool useBatchApi)
        {
            var events = Enumerable.Range(0, eventCount).Select(n => $"THIS IS EVENT NUMBER {n}");

            var topic = _factory.GetNewTopic();

            var producer = Using(
                _factory.ConfigureProducer()
                    .Topics(t => t.Map<string>(topic))
                    .Create()
            );

            if (!useBatchApi)
            {
                foreach (var e in events)
                {
                    await producer.Send(new ToposMessage(e), partitionKey: "whatever");
                }
            }
            else
            {
                //await producer.SendMany(events.Select(e => new ToposMessage(e)), partitionKey: "whatever");
            }

            var queue = new ConcurrentQueue<string>();

            var consumer = Using(
                _factory
                    .ConfigureConsumer("default")

                    .Create()
            );
        }
    }
}