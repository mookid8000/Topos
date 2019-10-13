using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Testy;
using Testy.Files;
using Topos.Config;
using Topos.Logging.Console;
using Topos.Producer;
using Topos.Tests.Contracts.Extensions;
// ReSharper disable ArgumentsStyleAnonymousFunction

#pragma warning disable 1998

namespace Topos.Kafkaesque.Tests
{
    [TestFixture]
    public class TestMaxQueueLengthCustomization : FixtureBase
    {
        TemporaryTestDirectory _testDirectory;
        IToposProducer _producer;

        protected override void SetUp()
        {
            _testDirectory = Using(new TemporaryTestDirectory());

            _producer = Configure.Producer(x => x.UseFileSystem(_testDirectory))
                .Logging(l => l.UseConsole(minimumLogLevel: LogLevel.Info))
                .Topics(t => t.Map<string>("all"))
                .Create();

            Using(_producer);
        }

        [TestCase(100)]
        [TestCase(1000)]
        [TestCase(5000)]
        [TestCase(10000)]
        [TestCase(20000)]
        public async Task CanCustomizeHowManyEventsGetDispatchedEachTime(int maximumBatchSize)
        {
            var encounteredBatchSizes = new ConcurrentQueue<int>();

            var consumer = Configure.Consumer("default", c => c.UseFileSystem(_testDirectory))
                .Logging(l => l.UseConsole(minimumLogLevel: LogLevel.Info))
                .Topics(t => t.Subscribe("all"))
                .Positions(p => p.StoreInMemory())
                .Options(o => o.SetMaximumBatchSize(maximumBatchSize))
                .Handle(async (batch, context, cancellationToken) => encounteredBatchSizes.Enqueue(batch.Count))
                .Create();

            Using(consumer);

            const int totalCount = 20000;

            var messages = Enumerable.Range(0, totalCount).Select(n => $"THIS IS MESSAGE NUMBNER {n}");

            using (TimerScope("send", totalCount))
            {
                await Task.WhenAll(messages.Select(m => _producer.Send(m)));
            }

            consumer.Start();

            using (TimerScope("receive", totalCount))
            {
                await encounteredBatchSizes
                    .WaitOrDie(
                        completionExpression: q => q.Sum() == totalCount,
                        failExpression: q => q.Sum() > totalCount,
                        failureDetailsFunction: () => $@"The sum is {encounteredBatchSizes.Sum()}",
                        timeoutSeconds: 60
                    );
            }

            Assert.That(encounteredBatchSizes.All(c => c <= maximumBatchSize), Is.True, $@"Expected all encountered batch sizes to be below {maximumBatchSize}, but we got these:

{string.Join(Environment.NewLine, encounteredBatchSizes.Select(e => $"    {e}" + (e > maximumBatchSize ? " !!!!!!!!!" : "")))}
");
        }
    }
}