using System;
using System.Collections.Concurrent;
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

namespace Topos.Faster.Tests;

[TestFixture]
public class TestAzureBlobsDeviceAndResumption : FixtureBase
{
    string _containerName;

    protected override void SetUp()
    {
        base.SetUp();

        _containerName = Guid.NewGuid().ToString();

        Using(new StorageContainerDeleter(_containerName));
    }

    [TestCase(2)]
    [TestCase(3)]
    [TestCase(10)]
    [TestCase(30)]
    [TestCase(100)]
    public async Task CanResumeAfterRestarting_NoReuse(int numberOfRestarts)
    {
        const string connectionString = BlobStorageDeviceManagerFactory.StorageConnectionString;
        
        // the only thing that survives between iterations in the in-mem positions storage
        var positions = new InMemPositionsStorage();

        // reduce amount of logging if running many iterations
        var logLevel = numberOfRestarts < 10 ? LogLevel.Debug : LogLevel.Info;

        var receivedMessages = new ConcurrentQueue<string>();

        IToposProducer CreateProducer() => Configure
            .Producer(s => s.UseAzureStorage(connectionString, _containerName))
            .Logging(l => l.UseConsole(logLevel))
            .Serialization(s => s.UseNewtonsoftJson())
            .Create();

        IDisposable CreateConsumer() => Configure
            .Consumer("default", s => s.UseAzureStorage(connectionString, _containerName))
            .Logging(l => l.UseConsole(logLevel))
            .Serialization(s => s.UseNewtonsoftJson())
            .Positions(p =>
            {
                p.StoreInMemory(positions);

                // decorate positions manager to log positions whenever they're loaded/saved
                StandardConfigurer.Open(p)
                    .Decorate(c => new ConsoleLoggingPositionsManagerDecorator(c.Get<IPositionManager>()));
            })
            .Topics(t => t.Subscribe("topic1"))
            .Handle(async (messages, _, _) => receivedMessages.EnqueueRange(messages.Select(m => m.Body).OfType<string>()))
            .Start();

        for (var counter = 1; counter <= numberOfRestarts; counter++)
        {
            using var consumer = CreateConsumer();
            using var producer = CreateProducer();

            await producer.Send("topic1", new($"besked {counter}"));

            await receivedMessages.WaitOrDie(q => q.Count == counter, failExpression: q => q.Count > counter);
        }

        Assert.That(receivedMessages, Is.EqualTo(Enumerable.Range(1, numberOfRestarts).Select(n => $"besked {n}")));
    }

    [TestCase(2)]
    [TestCase(3)]
    [TestCase(10)]
    [TestCase(30)]
    [TestCase(100)]
    [TestCase(1000)]
    public async Task CanResumeAfterRestarting_ReuseProducer(int numberOfRestarts)
    {
        const string connectionString = BlobStorageDeviceManagerFactory.StorageConnectionString;
        
        // the only thing that survives between iterations in the in-mem positions storage
        var positions = new InMemPositionsStorage();

        // reduce amount of logging if running many iterations
        var logLevel = numberOfRestarts < 10 ? LogLevel.Debug : LogLevel.Info;

        var receivedMessages = new ConcurrentQueue<string>();

        IToposProducer CreateProducer() => Configure
            .Producer(s => s.UseAzureStorage(connectionString, _containerName))
            .Logging(l => l.UseConsole(logLevel))
            .Serialization(s => s.UseNewtonsoftJson())
            .Create();

        IDisposable CreateConsumer() => Configure
            .Consumer("default", s => s.UseAzureStorage(connectionString, _containerName))
            .Logging(l => l.UseConsole(logLevel))
            .Serialization(s => s.UseNewtonsoftJson())
            .Positions(p =>
            {
                p.StoreInMemory(positions);

                // decorate positions manager to log positions whenever they're loaded/saved
                StandardConfigurer.Open(p)
                    .Decorate(c => new ConsoleLoggingPositionsManagerDecorator(c.Get<IPositionManager>()));
            })
            .Topics(t => t.Subscribe("topic1"))
            .Handle(async (messages, _, _) => receivedMessages.EnqueueRange(messages.Select(m => m.Body).OfType<string>()))
            .Start();

        using var producer = CreateProducer();

        for (var counter = 1; counter <= numberOfRestarts; counter++)
        {
            using var consumer = CreateConsumer();

            await producer.Send("topic1", new($"besked {counter}"));

            await receivedMessages.WaitOrDie(q => q.Count == counter, failExpression: q => q.Count > counter);
        }

        Assert.That(receivedMessages, Is.EqualTo(Enumerable.Range(1, numberOfRestarts).Select(n => $"besked {n}")));
    }
}