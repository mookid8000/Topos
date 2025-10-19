using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Testy;
using Testy.Files;
using Topos.Config;
using Topos.Internals;
using Topos.Producer;
// ReSharper disable UnusedParameter.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
#pragma warning disable CS1998

namespace Topos.Faster.Tests;

[TestFixture]
public class VerifyCompaction : FixtureBase
{
    [Test]
    public async Task CanDoIt()
    {
        var testDirectory = NewTempDirectory();

        using var producer = CreateProducer(testDirectory);

        await producer.SendManyAsync("test-topic", Enumerable.Range(0, 1000).Select(n => new ToposMessage(new Timestamp($"1-{n}", DateTimeOffset.Now))));
        await Task.Delay(TimeSpan.FromSeconds(3));

        await producer.SendManyAsync("test-topic", Enumerable.Range(0, 1000).Select(n => new ToposMessage(new Timestamp($"2-{n}", DateTimeOffset.Now))));
        await Task.Delay(TimeSpan.FromSeconds(3));

        using var consumer1 = StartConsumer(testDirectory, message =>
        {
            //Console.WriteLine($"Got event {message.Label}: {message.Time}")
        });

        await Task.Delay(TimeSpan.FromSeconds(60));

        await producer.SendManyAsync("test-topic", Enumerable.Range(0, 1000).Select(n => new ToposMessage(new Timestamp($"3-{n}", DateTimeOffset.Now))));

        await Task.Delay(TimeSpan.FromSeconds(60));

        using var consumer2 = StartConsumer(testDirectory, message =>
        {
            //Console.WriteLine($"Got event {message.Label}: {message.Time}");
        });

        await Task.Delay(TimeSpan.FromSeconds(10));
    }

    record Timestamp(string Label, DateTimeOffset Time);

    static IToposProducer CreateProducer(TemporaryTestDirectory temporaryTestDirectory)
    {
        return Configure
            .Producer(p =>
            {
                p.UseFileSystem(temporaryTestDirectory).SetMaxAge("test-topic", TimeSpan.FromSeconds(5));

                ChangeCompactionIntervalTo(p, TimeSpan.FromSeconds(1));
            })
            .Serialization(s => s.UseNewtonsoftJson())
            .Create();
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

    static IDisposable StartConsumer(TemporaryTestDirectory testDirectory, Action<Timestamp> handle)
    {
        return Configure
            .Consumer("whatever", c => c.UseFileSystem(testDirectory))
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
}