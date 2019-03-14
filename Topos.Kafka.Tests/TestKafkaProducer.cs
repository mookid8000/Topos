using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Topos.Logging.Console;
using Topos.Tests;
using Topos.Tests.Extensions;
// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleStringLiteral

namespace Topos.Kafka.Tests
{
    [TestFixture]
    public class TestKafkaProducer : ToposFixtureBase
    {
        //KafkaProducer _producer;

        //protected override void SetUp()
        //{
        //    Logger.Information("Setting up");

        //    _producer = new KafkaProducer(
        //        loggerFactory: new ConsoleLoggerFactory(),
        //        address: KafkaTestConfig.Address
        //    );

        //    Using(_producer);

        //    Logger.Information("Producer initialized");
        //}

        //[Test]
        ////[Ignore("hej")]
        //public async Task CanSendEvents()
        //{
        //    Logger.Information("Sending events");

        //    await Time.Action("produce", async () =>
        //    {
        //        await _producer.SendAsync("test-topic",
        //            new[]
        //            {
        //                new KafkaEvent("key1", "hej"),
        //                new KafkaEvent("key2", "med"),
        //                new KafkaEvent("key2", "dig")
        //            });
        //    });

        //    Logger.Information("Successfully sent");
        //}

        //[TestCase(100000)]
        //[Ignore("hej")]
        //public async Task CanSendEvents_Lots(int count)
        //{
        //    Logger.Information("Sending events");

        //    await Time.Action("produce", async () =>
        //    {
        //        var messages = Enumerable.Range(0, count)
        //            .Select(n => new KafkaEvent($"key-{n % 64}", $"det her er besked nr {n}"));

        //        foreach (var batch in messages.Batch(100))
        //        {
        //            await _producer.SendAsync("lots", batch);
        //        }
        //    });

        //    Logger.Information("Successfully sent");
        //}
    }
}
