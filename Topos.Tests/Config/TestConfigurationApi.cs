using System.Threading.Tasks;
using NUnit.Framework;
using Testy;
using Topos.Config;
using Topos.SqlServer.Config;

namespace Topos.Tests.Config
{
    [TestFixture]
    [Ignore("wait with this")]
    public class TestConfigurationApi : FixtureBase
    {
        [Test]
        public void CanConfigure_Logging()
        {
            Using(Configure.Consumer()
                .Logging(l => l.UseSerilog())
                .EventBroker(t => t.UseSqlServer("server=.; database=topos_test; trusted_connection=true"))
                .Start());
        }

        [Test]
        public void CanConfigure_Consumer_Sql()
        {
            var consumer = Configure.Consumer()
                .EventBroker(t => t.UseSqlServer("server=.; database=topos_test; trusted_connection=true"))
                .Start();

            Using(consumer);
        }

        [Test]
        public async Task CanConfigure_Producer_Sql()
        {
            var producer = Configure.Producer()
                .EventBroker(t => t.UseSqlServer("server=.; database=topos_test; trusted_connection=true"))
                .Create();

            await producer.Send("hej med dig");
        }

        [Test]
        public void CanConfigure_Consumer_AzureEventHubs()
        {
            var disposable = Configure.Consumer()
                .EventBroker(t => t.UseAzureEventHubs(AehConfig.ConnectionString))
                .Start();

            Using(disposable);
        }

        [Test]
        public async Task CanConfigure_Producer_AzureEventHubs()
        {
            var producer = Configure.Producer()
                .EventBroker(t => t.UseAzureEventHubs(AehConfig.ConnectionString))
                .Create();

            await producer.Send("hej med dig");
        }
    }
}