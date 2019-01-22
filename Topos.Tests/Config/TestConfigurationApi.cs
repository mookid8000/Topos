using System.Threading.Tasks;
using NUnit.Framework;
using Topos.Config;
using Topos.SqlServer.Config;

namespace Topos.Tests.Config
{
    [TestFixture]
    public class TestConfigurationApi : FixtureBase
    {
        [Test]
        public void CanConfigure_Consumer_Sql()
        {
            var disposable = Configure.Consumer()
                .EventStore(t => t.UseSqlServer("server=.; database=topos_test; trusted_connection=true"))
                .Start();

            Using(disposable);
        }

        [Test]
        public async Task CanConfigure_Producer_Sql()
        {
            var producer = Configure.Producer()
                .EventStore(t => t.UseSqlServer("server=.; database=topos_test; trusted_connection=true"))
                .Create();

            await producer.Send("hej med dig");
        }

        [Test]
        public void CanConfigure_Consumer_AzureEventHubs()
        {
            var disposable = Configure.Consumer()
                .EventStore(t => t.UseAzureEventHubs(AehConfig.ConnectionString))
                .Start();

            Using(disposable);
        }

        [Test]
        public async Task CanConfigure_Producer_AzureEventHubs()
        {
            var producer = Configure.Producer()
                .EventStore(t => t.UseAzureEventHubs(AehConfig.ConnectionString))
                .Create();

            await producer.Send("hej med dig");
        }
    }
}