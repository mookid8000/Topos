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
            var producer = Configure.Producer()
                .Logging(l => l.UseSerilog())
                .EventBroker(e => e.UseKafka(KafkaTestConfig.Address))
                .Create();

            Using(producer);

            var consumer = Configure.Consumer()
                .Logging(l => l.UseSerilog())
                .EventBroker(e => e.UseKafka(KafkaTestConfig.Address))
                .Create();

            Using(consumer);
        }
    }
}