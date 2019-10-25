using System.Threading.Tasks;
using NUnit.Framework;
using Testy;
using Topos.Config;
using Topos.Logging.Console;
using Topos.Producer;
using Topos.SqlServer.Config;

namespace Topos.SqlServer.Tests
{
    [TestFixture]
    [Ignore("wait with this")]
    public class TestConfigurationApi : FixtureBase
    {
        [Test]
        public void CanConfigure_Consumer_AzureEventHubs()
        {
            var disposable = Configure.Consumer("default-group", t => t.UseSqlServer("server=.; database=topoc; trusted_connection=true"))
                .Logging(l => l.UseConsole())
                .Start();

            Using(disposable);
        }

        [Test]
        public async Task CanConfigure_Producer_AzureEventHubs()
        {
            var producer = Configure.Producer(t => t.UseSqlServer("server=.; database=topoc; trusted_connection=true"))
                .Logging(l => l.UseConsole())
                .Create();

            await producer.Send("some-topic", new ToposMessage("hej med dig"));
        }
    }
}