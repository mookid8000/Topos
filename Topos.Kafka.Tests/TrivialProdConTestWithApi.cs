using System.Threading.Tasks;
using NUnit.Framework;
using Topos.Config;

namespace Topos.Kafka.Tests
{
    [TestFixture]
    public class TrivialProdConTestWithApi : KafkaFixtureBase
    {
        string _topic;

        protected override void SetUp()
        {
            _topic = GetNewTopic();
        }

        [Test]
        public async Task ItWorks()
        {
            var producer = Configure
                .Producer(e => e.UseKafka(KafkaTestConfig.Address))
                .Logging(l => l.UseSerilog())
                .Create();

            Using(producer);

            var consumer = Configure
                .Consumer(e => e.UseKafka(KafkaTestConfig.Address))
                .Logging(l => l.UseSerilog())
                .Start();

            Using(consumer);
        }
    }
}