using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Testy;
using Testy.Extensions;
using Topos.Config;
using Topos.Consumer;
using Topos.Faster.Tests.Factories;
using Topos.InMem;
using Topos.Logging.Console;
using Topos.Producer;
#pragma warning disable CS1998

namespace Topos.Faster.Tests.Bugs;

[TestFixture]
public class TryReproduceOddInabilityToCatchUp : FixtureBase
{
    string _containerName;
    string _connectionString;
    LogLevel _logLevel;
    InMemPositionsStorage _positionsStorage;

    protected override void SetUp()
    {
        base.SetUp();

        _containerName = Guid.NewGuid().ToString();

        Using(new StorageContainerDeleter(_containerName));

        _connectionString = BlobStorageDeviceManagerFactory.StorageConnectionString;
        
        _logLevel = LogLevel.Debug;

        _positionsStorage = new InMemPositionsStorage();
    }

    [Test]
    public async Task JustStartTheOne_Producer()
    {
        using var producer = CreateProducer();

        await producer.SendAsync(topic: "topic2", new("hej"));

    }

    [Test]
    public async Task JustStartTheOne_Consumer()
    {
        using var consumer = CreateConsumer(handleStrings: _ => { });

    }

    [TestCase(10)]
    [TestCase(100)]
    [TestCase(1000)]
    public async Task FirstProducerThenConsumer(int count)
    {
        var receivedStrings = new ConcurrentQueue<string>();

        using var producer = CreateProducer();
        using var consumer = CreateConsumer(handleStrings: receivedStrings.EnqueueRange);

        await producer.SendManyAsync(topic: "topic1", messages: GetMessages(count));

        await receivedStrings.WaitOrDie(
            completionExpression: q => q.Count == count,
            failExpression: q => q.Count > count,
            timeoutSeconds: 10
        );
    }

    [TestCase(10)]
    [TestCase(100)]
    [TestCase(1000)]
    public async Task FirstConsumerThenProducer(int count)
    {
        var receivedStrings = new ConcurrentQueue<string>();

        using var consumer = CreateConsumer(handleStrings: receivedStrings.EnqueueRange);
        using var producer = CreateProducer();

        await producer.SendManyAsync(topic: "topic1", messages: GetMessages(count));

        await receivedStrings.WaitOrDie(
            completionExpression: q => q.Count == count,
            failExpression: q => q.Count > count,
            timeoutSeconds: 10
        );
    }

    static IEnumerable<ToposMessage> GetMessages(int count) => Enumerable.Range(start: 0, count: count).Select(selector: n => new ToposMessage(body: $"THIS IS MESSAGE NUMBER {n}"));

    IToposProducer CreateProducer() => Configure
        .Producer(s => s.UseAzureStorage(_connectionString, _containerName))
        .Logging(l => l.UseConsole(_logLevel))
        .Serialization(s => s.UseNewtonsoftJson())
        .Create();

    IDisposable CreateConsumer(Action<IReadOnlyList<string>> handleStrings) => Configure
        .Consumer("default", s => s.UseAzureStorage(_connectionString, _containerName))
        .Logging(l => l.UseConsole(_logLevel))
        .Serialization(s => s.UseNewtonsoftJson())
        .Positions(p =>
        {
            p.StoreInMemory(_positionsStorage);

            // decorate positions manager to log positions whenever they're loaded/saved
            StandardConfigurer.Open(p)
                .Decorate(c => new ConsoleLoggingPositionsManagerDecorator(c.Get<IPositionManager>()));
        })
        .Topics(t => t.Subscribe("topic1"))
        .Handle(async (messages, _, _) => handleStrings(messages.Select(m => m.Body).OfType<string>().ToList()))
        .Start();

}