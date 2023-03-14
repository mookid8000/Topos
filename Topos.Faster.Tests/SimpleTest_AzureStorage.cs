using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Testy;
using Topos.Config;
using Topos.Faster.Tests.Factories;
using Topos.Producer;
using Topos.Tests.Contracts.Extensions;
#pragma warning disable 1998

namespace Topos.Faster.Tests;

[TestFixture]
public class SimpleTest_AzureStorage : FixtureBase
{
    string _containerName;

    protected override void SetUp()
    {
        base.SetUp();
        _containerName = Guid.NewGuid().ToString("N");
        Using(new StorageContainerDeleter(_containerName));
    }

    [Test]
    public async Task CanProduceSomeEvents_Produce_then_consume()
    {
        using var gotTheEvent = new ManualResetEvent(initialState: false);
        var testDirectory = NewTempDirectory();

        using var producer = CreateProducer();
        await producer.Send("test-topic", new ToposMessage(new SomeMessage()));

        using var consumer = StartConsumer(gotTheEvent);

        gotTheEvent.WaitOrDie(errorMessage: "Did not get the expected events callback");
    }

    [Test]
    public async Task CanProduceSomeEvents_Consume_then_produce()
    {
        using var gotTheEvent = new ManualResetEvent(initialState: false);
        var testDirectory = NewTempDirectory();

        using var consumer = StartConsumer(gotTheEvent);

        using var producer = CreateProducer();
        await producer.Send("test-topic", new ToposMessage(new SomeMessage()));

        gotTheEvent.WaitOrDie(errorMessage: "Did not get the expected events callback");
    }

    IToposProducer CreateProducer()
    {
        return Configure
            .Producer(p => p.UseAzureStorage(BlobStorageDeviceManagerFactory.StorageConnectionString, _containerName, "faster"))
            .Serialization(s => s.UseNewtonsoftJson())
            .Create();
    }

    IDisposable StartConsumer(ManualResetEvent gotTheEvent)
    {
        return Configure
            .Consumer("whatever", c => c.UseAzureStorage(BlobStorageDeviceManagerFactory.StorageConnectionString, _containerName, "faster"))
            .Serialization(s => s.UseNewtonsoftJson())
            .Topics(t => t.Subscribe("test-topic"))
            .Positions(p => p.StoreInMemory())
            .Handle(async (messages, context, token) => gotTheEvent.Set())
            .Start();
    }

    class SomeMessage { }
}