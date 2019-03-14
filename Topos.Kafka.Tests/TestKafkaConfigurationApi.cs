using System.Threading.Tasks;
using NUnit.Framework;
using Testy;
using Topos.Config;

namespace Topos.Kafka.Tests
{
    [TestFixture]
    public class TestKafkaConfigurationApi : FixtureBase
    {
        [Test]
        public async Task FullProducerConsumerExample()
        {
            var producer = Configure.Producer(e => e.UseKafka(KafkaTestConfig.Address))
                .Logging(l => l.UseSerilog())
                .Create();

            Using(producer);

            var consumer = Configure.Consumer(e => e.UseKafka(KafkaTestConfig.Address))
                .Logging(l => l.UseSerilog())
                .Create();

            Using(consumer);
        }
    }
}