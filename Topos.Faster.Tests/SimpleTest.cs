using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Testy;
using Topos.Config;
using Topos.Producer;
using Topos.Tests.Contracts.Extensions;
#pragma warning disable 1998

namespace Topos.Faster.Tests
{
    [TestFixture]
    public class SimpleTest : FixtureBase
    {
        [Test]
        public async Task CanProduceSomeEvents()
        {
            var temporaryTestDirectory = NewTempDirectory();

            using var producer = Configure
                .Producer(p => p.UseFileSystem(temporaryTestDirectory))
                .Serialization(s => s.UseNewtonsoftJson())
                .Create();

            await producer.Send("test-topic", new ToposMessage(new SomeMessage()));

            using var gotTheEvent = new ManualResetEvent(initialState: false);

            using var consumer = Configure
                .Consumer("whatever", c => c.UseFileSystem(temporaryTestDirectory))
                .Serialization(s => s.UseNewtonsoftJson())
                .Topics(t => t.Subscribe("test-topic"))
                .Positions(p => p.StoreInMemory())
                .Handle(async (messages, context, token) => gotTheEvent.Set())
                .Start();

            gotTheEvent.WaitOrDie(errorMessage: "Did not get the expected events callback");
        }

        class SomeMessage { }
    }
}