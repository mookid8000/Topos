using System.Threading.Tasks;
using NUnit.Framework;
using Testy;
using Topos.AzureEventHubs.Tests;
using Topos.Config;
using Topos.Logging.Console;

namespace Topos.Tests.Config
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
                .EventBroker(t => t.UseAzureEventHubs(AehConfig.ConnectionString))
                .Start();

            Using(disposable);
        }

        [Test]
        public async Task CanConfigure_Producer_AzureEventHubs()
        {
            var producer = Configure.Producer()
                .Logging(l => l.UseConsole())
                .EventBroker(t => t.UseAzureEventHubs(AehConfig.ConnectionString))
                .Create();

            await producer.Send("hej med dig");
        }
    }
}