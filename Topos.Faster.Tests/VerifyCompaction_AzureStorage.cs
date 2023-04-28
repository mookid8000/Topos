using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Testy;
using Topos.Config;
using Topos.Faster.Tests.Factories;
using Topos.Internals;
using Topos.Producer;
// ReSharper disable UnusedParameter.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
#pragma warning disable CS1998

namespace Topos.Faster.Tests;

[TestFixture]
public class VerifyCompaction_AzureStorage : FixtureBase
{
    string _containerName;

    protected override void SetUp()
    {
        base.SetUp();

        _containerName = Guid.NewGuid().ToString("N");

        Using(new StorageContainerDeleter(_containerName));
    }

    [Test]
    public async Task CanDoIt()
    {
        using var producer = CreateProducer();

        await producer.SendMany("test-topic", Enumerable.Range(0, 1000).Select(n => new ToposMessage(new Timestamp($"1-{n}", DateTimeOffset.Now))));
        await Task.Delay(TimeSpan.FromSeconds(3));

        await producer.SendMany("test-topic", Enumerable.Range(0, 1000).Select(n => new ToposMessage(new Timestamp($"2-{n}", DateTimeOffset.Now))));
        await Task.Delay(TimeSpan.FromSeconds(3));

        using var consumer1 = StartConsumer(message =>
        {
            //Console.WriteLine($"Got event {message.Label}: {message.Time}")
        });

        await Task.Delay(TimeSpan.FromSeconds(60));

        await producer.SendMany("test-topic", Enumerable.Range(0, 1000).Select(n => new ToposMessage(new Timestamp($"3-{n}", DateTimeOffset.Now))));

        await Task.Delay(TimeSpan.FromSeconds(60));

        using var consumer2 = StartConsumer(message =>
        {
            //Console.WriteLine($"Got event {message.Label}: {message.Time}");
        });

        await Task.Delay(TimeSpan.FromSeconds(10));
    }

    class Timestamp
    {
        public Timestamp(string label, DateTimeOffset time)
        {
            Label = label;
            Time = time;
        }

        public string Label { get; }
        public DateTimeOffset Time { get; }
    }

    IToposProducer CreateProducer()
    {
        return Configure
            .Producer(p =>
            {
                p.UseAzureStorage(BlobStorageDeviceManagerFactory.StorageConnectionString, _containerName)
                    .SetMaxAge("test-topic", TimeSpan.FromSeconds(5));

                ChangeCompactionIntervalTo(p, TimeSpan.FromSeconds(1));
            })
            .Serialization(s => s.UseNewtonsoftJson())
            .Create();
    }

    IDisposable StartConsumer(Action<Timestamp> handle)
    {
        return Configure
            .Consumer("whatever", c => c.UseAzureStorage(BlobStorageDeviceManagerFactory.StorageConnectionString, _containerName))
            .Serialization(s => s.UseNewtonsoftJson())
            .Topics(t => t.Subscribe("test-topic"))
            .Positions(p => p.StoreInMemory())
            .Handle(async (messages, context, token) =>
            {
                foreach (var message in messages.Select(m => m.Body).OfType<Timestamp>())
                {
                    handle(message);
                }
            })
            .Start();
    }

    static void ChangeCompactionIntervalTo(StandardConfigurer<IProducerImplementation> p, TimeSpan interval)
    {
        StandardConfigurer.Open(p)
            .Other<EventExpirationHelper>()
            .Decorate(c =>
            {
                var helper = c.Get<EventExpirationHelper>();
                helper.CompactionInterval = interval;
                return helper;
            });
    }
}