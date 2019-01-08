using System.Threading.Tasks;
using NUnit.Framework;
using Topos.Config;
using Topos.SqlServer.Config;

namespace Topos.Tests.Config
{
    [TestFixture]
    [Ignore("save for later")]
    public class TestConfigurationApi : FixtureBase
    {
        [Test]
        public void CanConfigure_Consumer()
        {
            Configure.Consumer()
                .EventStore(t => t.UseSqlServer("server=.; database=topos_test; trusted_connection=true"))
                .Start();
        }

        [Test]
        public async Task CanConfigure_Producer()
        {
            var producer = Configure.Producer()
                .EventStore(t => t.UseSqlServer("server=.; database=topos_test; trusted_connection=true"))
                .Create();

            await producer.Send("hej med dig");
        }
    }
}