using System.Threading.Tasks;
using NUnit.Framework;
using Testy;
using Topos.Config;
using Topos.Producer;

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
        }

        class SomeMessage { }
    }
}