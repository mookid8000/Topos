using System.Threading.Tasks;
using NUnit.Framework;
using Testy;
using Topos.Broker.InMem;
using Topos.Config;
using Topos.EventProcessing;

namespace Topos.Tests.Config
{
    [TestFixture]
    public class TestTrivialProducerConsumer : FixtureBase
    {
        [Test]
        public async Task ItWorks()
        {
            var eventStore = new InMemEventBroker();

            var producer = Configure.Producer()
                .EventBroker(e => e.UseInMemory(eventStore))
                .Create();

            var consumer = Configure.Consumer()
                .EventBroker(e => e.UseInMemory(eventStore))
                //.EventProcessing(p => p.AddEventProcessor(new ConsoleEventProcessor()))
                .Start();

            Using(consumer);

            await producer.Send("HEJ MED DIG MIN VEN");
        }
    }

    public class ConsoleEventProcessor : IEventProcessor
    {
    }
}