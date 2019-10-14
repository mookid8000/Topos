using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Testy.Benchmarking;
using Topos.Config;
using Topos.Tests.Contracts.Extensions;
using Topos.Tests.Contracts.Factories;
// ReSharper disable ArgumentsStyleAnonymousFunction
#pragma warning disable 1998

namespace Topos.Tests.Contracts.Broker
{
    public abstract class MaxQueueLengthCustomizationTest<TBrokerFactory> : ToposContractFixtureBase where TBrokerFactory : IBrokerFactory, new()
    {
        TBrokerFactory _factory;

        protected override void AdditionalSetUp()
        {
            _factory = Using(new TBrokerFactory());
        }

        [TestCase(100, 100)]
        [TestCase(1000, 1000)]
        [TestCase(5000, 5000)]
        [TestCase(10000, 10000)]
        [TestCase(20000, 20000)]
        public async Task CanCustomizeHowManyEventsGetDispatchedEachTime(int minimumBatchSize, int maximumBatchSize)
        {
            var topic = _factory.GetNewTopic();

            var producer = _factory.ConfigureProducer()
                .Topics(t => t.Map<string>(topic))
                .Create();

            Using(producer);

            var encounteredBatchSizes = new ConcurrentQueue<int>();

            var consumer = _factory.ConfigureConsumer("default")
                .Topics(t => t.Subscribe(topic))
                .Positions(p => p.StoreInMemory())
                .Options(o =>
                {
                    o.SetMinimumBatchSize(minimumBatchSize);
                    o.SetMaximumBatchSize(maximumBatchSize);
                })
                .Handle(async (batch, context, cancellationToken) => encounteredBatchSizes.Enqueue(batch.Count))
                .Create();

            Using(consumer);

            const int totalCount = 20000; // remember this one must be a multiple of the minimum batch size!!!

            var messages = Enumerable.Range(0, totalCount).Select(n => $"THIS IS MESSAGE NUMBNER {n}");

            using (new TimerScope("send", totalCount))
            {
                await Task.WhenAll(messages.Select(m => producer.Send(m)));
            }

            consumer.Start();

            string FormatBatchSizes() => string.Join(Environment.NewLine, encounteredBatchSizes
                    .Select(e => $"    {e}" + ((e > maximumBatchSize || e < minimumBatchSize) ? " !!!!!!!!!" : "")));

            using (new TimerScope("receive", totalCount))
            {
                await encounteredBatchSizes
                    .WaitOrDie(
                        completionExpression: q => q.Sum() == totalCount,
                        failExpression: q => q.Sum() > totalCount,
                        failureDetailsFunction: () => $@"The sum is {encounteredBatchSizes.Sum()}

All the batch sizes are here:

{FormatBatchSizes()}
",
                        timeoutSeconds: 120
                    );
            }

            Assert.That(encounteredBatchSizes.All(c => c <= maximumBatchSize && c >= minimumBatchSize), Is.True,
                $@"Expected all encountered batch sizes N to satisfy {minimumBatchSize} <= N <= {maximumBatchSize}, but we got these:

{FormatBatchSizes()}
");
        }
    }
}