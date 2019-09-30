using System.Threading.Tasks;
using NUnit.Framework;
using Testy;
using Topos.Config;
using Topos.InMem;

namespace Topos.Tests.Config
{
    [TestFixture]
    public class TestTrivialProducerConsumer : FixtureBase
    {
        [Test]
        [Ignore("not implemented yet")]
        public async Task ItWorks()
        {
            var eventBroker = new InMemEventBroker();

            var producer = Configure.Producer(e => e.UseInMemory(eventBroker))
                .Create();

            var consumer = Configure.Consumer("default-group", e => e.UseInMemory(eventBroker))
                .Start();

            Using(consumer);

            await producer.Send("HEJ MED DIG MIN VEN");
        }
    }
}