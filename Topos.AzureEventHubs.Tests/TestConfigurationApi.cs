using System.Threading.Tasks;
using NUnit.Framework;
using Testy;
using Topos.Config;
using Topos.Logging.Console;
using Topos.Producer;

namespace Topos.AzureEventHubs.Tests
{
    [TestFixture]
    [Ignore("wait with this")]
    public class TestConfigurationApi : FixtureBase
    {
        [Test]
        public void CanConfigure_Consumer_AzureEventHubs()
        {
            var disposable = Configure.Consumer("default-group", t => t.UseAzureEventHubs(AehConfig.ConnectionString))
                .Logging(l => l.UseConsole())
                .Start();

            Using(disposable);
        }

        [Test]
        public async Task CanConfigure_Producer_AzureEventHubs()
        {
            var producer = Configure.Producer(t => t.UseAzureEventHubs(AehConfig.ConnectionString))
                .Logging(l => l.UseConsole())
                .Create();

            await producer.Send(new ToposMessage("hej med dig"));
        }
    }
}