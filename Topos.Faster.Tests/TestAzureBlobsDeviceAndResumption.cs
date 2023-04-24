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
    public async Task CanResumeAfterRestarting(int numberOfRestarts)
    {
        var positions = new InMemPositionsStorage();

        using var producer = Configure
            .Producer(s => s.UseAzureStorage(BlobStorageDeviceManagerFactory.StorageConnectionString, _containerName, "directory"))
            .Serialization(s => s.UseNewtonsoftJson())
            .Create();

        var receivedMessages = new ConcurrentQueue<string>();

        IDisposable StartConsumer() => Configure
            .Consumer("default", s => s.UseAzureStorage(BlobStorageDeviceManagerFactory.StorageConnectionString, _containerName, "directory"))
            .Serialization(s => s.UseNewtonsoftJson())
            .Positions(p =>
            {
                p.StoreInMemory(positions);
                
                StandardConfigurer.Open(p)
                    .Decorate(c => new ConsoleLoggingPositionsManagerDecorator(c.Get<IPositionManager>()));
            })
            .Topics(t => t.Subscribe("topic1"))
            .Handle(async (messages, _, _) => receivedMessages.EnqueueRange(messages.Select(m => m.Body).OfType<string>()))
            .Start();

        for (var counter = 1; counter <= numberOfRestarts; counter++)
        {
            using (StartConsumer())
            {
                await producer.Send("topic1", new($"besked {counter}"));

                await receivedMessages.WaitOrDie(q => q.Count == counter, failExpression: q => q.Count > counter);
            }
        }

        Assert.That(receivedMessages, Is.EqualTo(Enumerable.Range(1, numberOfRestarts).Select(n => $"besked {n}")));
    }

    class ConsoleLoggingPositionsManagerDecorator : IPositionManager
    {
        readonly IPositionManager _positionManager;

        public ConsoleLoggingPositionsManagerDecorator(IPositionManager positionManager) => _positionManager = positionManager ?? throw new ArgumentNullException(nameof(positionManager));

        public async Task Set(Position position)
        {
            Console.WriteLine($"Setting position {position}");
            await _positionManager.Set(position);
        }

        public async Task<Position> Get(string topic, int partition)
        {
            var position = await _positionManager.Get(topic, partition);
            Console.WriteLine($"Got position {position}");
            return position;
        }
    }
}