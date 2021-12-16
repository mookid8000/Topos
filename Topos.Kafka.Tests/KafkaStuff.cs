using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Topos.Logging.Console;
using Topos.Tests;
#pragma warning disable 1998

namespace Topos.Kafka.Tests;

[TestFixture]
public class KafkaStuff : ToposFixtureBase
{
    [Test]
    public async Task ListTopics()
    {
        var producer = new KafkaProducerImplementation(new ConsoleLoggerFactory(minimumLogLevel: LogLevel.Debug), KafkaTestConfig.Address);

        Using(producer);

        var adminClient = producer.GetAdminClient();

        Using(adminClient);

        var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));

        Console.WriteLine($@"Topics:

{string.Join(Environment.NewLine, metadata.Topics.Select(t => $"    {t.Topic}: {string.Join(", ", t.Partitions.Select(p => p.PartitionId))}"))}

");

        //Console.WriteLine("Deleting all topics!");

        //await adminClient.DeleteTopicsAsync(metadata.Topics.Select(t => t.Topic));

    }
}