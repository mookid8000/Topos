using System.Threading.Tasks;
using NUnit.Framework;
using Testy;
using Topos.Config;
using Topos.Logging.Console;
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
            var disposable = Configure.Consumer()
                .Logging(l => l.UseConsole())
                .EventBroker(t => t.UseSqlServer("server=.; database=topoc; trusted_connection=true"))
                .Start();

            Using(disposable);
        }

        [Test]
        public async Task CanConfigure_Producer_AzureEventHubs()
        {
            var producer = Configure.Producer()
                .Logging(l => l.UseConsole())
                .EventBroker(t => t.UseSqlServer("server=.; database=topoc; trusted_connection=true"))
                .Create();

            await producer.Send("hej med dig");
        }
    }
}