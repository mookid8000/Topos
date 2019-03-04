using System.Threading.Tasks;
using NUnit.Framework;
using Testy;
using Topos.Config;

namespace Topos.Tests.Config
{
    [TestFixture]
    public class TestKafkaConfigurationApi : FixtureBase
    {
        [Test]
        public async Task CanGetKafkaProducer()
        {
            var producer = Configure.Producer()
                .Logging(l => l.UseSerilog())
                .EventBroker(e => e.UseKafka("localhost:9092"))
                .Create();

            Using(producer);
        }
    }
}