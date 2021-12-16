using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Testy.Benchmarking;
using Testy.Extensions;
using Testy.Timers;
using Topos.Config;
using Topos.Producer;
using Topos.Tests.Contracts.Factories;
// ReSharper disable ArgumentsStyleAnonymousFunction
#pragma warning disable 1998

namespace Topos.Tests.Contracts.Broker;

public abstract class MaxQueueLengthCustomizationTest<TBrokerFactory> : ToposContractFixtureBase where TBrokerFactory : IBrokerFactory, new()
{
    TBrokerFactory _factory;

    protected override void AdditionalSetUp()
    {
        _factory = Using(new TBrokerFactory());
    }

    [TestCase(1000, 1, 10000)]
    [TestCase(5, 1, 4)]
    [TestCase(50, 10, 10)]
    [TestCase(500, 100, 100)]
    [TestCase(5000, 1000, 1000)]
    [TestCase(20000, 5000, 5000)]
    [TestCase(20000, 10000, 10000)]
    [TestCase(20000, 20000, 20000)]
    public async Task CanCustomizeHowManyEventsGetDispatchedEachTime(int totalCount, int minimumBatchSize, int maximumBatchSize)
    {
        Assert.That(totalCount % minimumBatchSize, Is.EqualTo(0),
            "Please ensure that the total count is a multiple of the minimum batch size");

        var topic = _factory.GetNewTopic();

        var producer = _factory.ConfigureProducer().Create();

        Using(producer);

        var encounteredBatchSizes = new ConcurrentQueue<int>();

        var consumer = _factory.ConfigureConsumer("default")
            .Topics(t => t.Subscribe(topic))
            .Positions(p => p.StoreInMemory())
            .Options(o =>
            {
                o.SetMinimumBatchSize(minimumBatchSize);
                o.SetMaximumBatchSize(maximumBatchSize);
                o.SetMaximumPrefetchQueueLength(maximumBatchSize * 2);
            })
            .Handle(async (batch, context, cancellationToken) => encounteredBatchSizes.Enqueue(batch.Count))
            .Create();

        Using(consumer);

        var messages = Enumerable.Range(0, totalCount).Select(n => $"THIS IS MESSAGE NUMBNER {n}");

        using (new TimerScope($"send {totalCount} messages", totalCount))
        {
            await Task.WhenAll(messages.Select(m => producer.Send(topic, new ToposMessage(m))));
        }

        consumer.Start();

        string FormatBatchSizes() => string.Join(Environment.NewLine, encounteredBatchSizes
            .Select(e => $"    {e}" + ((e > maximumBatchSize || e < minimumBatchSize) ? " !!!!!!!!!" : "")));

        using (new TimerScope($"receive {totalCount} messages", totalCount))
        using (new PeriodicCallback(TimeSpan.FromSeconds(5), () => Console.WriteLine($"{DateTime.Now:HH:mm:ss} SUM(encountered batch sizes) = {encounteredBatchSizes.Sum()}")))
        {
            await encounteredBatchSizes
                .WaitOrDie(
                    completionExpression: q => q.Sum() == totalCount,
                    failExpression: q => q.Sum() > totalCount,
                    failureDetailsFunction: () => $@"The sum is {encounteredBatchSizes.Sum()}

All the batch sizes are here:

{FormatBatchSizes()}

SUM: {encounteredBatchSizes.Sum()}
",
                    timeoutSeconds: 20
                );
        }

        Assert.That(encounteredBatchSizes.All(c => c <= maximumBatchSize && c >= minimumBatchSize), Is.True,
            $@"Expected all encountered batch sizes N to satisfy {minimumBatchSize} <= N <= {maximumBatchSize}, but we got these:

{FormatBatchSizes()}

SUM: {encounteredBatchSizes.Sum()}
");
    }
}