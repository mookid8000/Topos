using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Topos.Logging;
using Topos.Tests;

namespace Topos.Kafka.Tests
{
    [TestFixture]
    public class KafkaStuff : ToposFixtureBase
    {
        [Test]
        public async Task ListTopics()
        {
            var producer = new KafkaProducer(new ConsoleLoggerFactory(), KafkaTestConfig.Address);

            Using(producer);

            var adminClient = producer.GetAdminClient();

            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));

            Console.WriteLine($@"Topics:

{string.Join(Environment.NewLine, metadata.Topics.Select(t => $"    {t.Topic}: {string.Join(", ", t.Partitions.Select(p => p.PartitionId))}"))}

");

            //Console.WriteLine("Deleting all topics!");

            //await adminClient.DeleteTopicsAsync(metadata.Topics.Select(t => t.Topic));

        }
    }
}