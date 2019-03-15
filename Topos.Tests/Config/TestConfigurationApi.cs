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
            var consumer = Configure.Consumer("default-group", t => t.UseSqlServer("server=.; database=topos_test; trusted_connection=true"))
                .Logging(l => l.UseSerilog())
                .Start();

            Using(consumer);
        }

        [Test]
        public void CanConfigure_Consumer_Sql()
        {
            var consumer = Configure.Consumer("default-group", t => t.UseSqlServer("server=.; database=topos_test; trusted_connection=true"))
                .Start();

            Using(consumer);
        }

        [Test]
        public async Task CanConfigure_Producer_Sql()
        {
            var producer = Configure.Producer(t => t.UseSqlServer("server=.; database=topos_test; trusted_connection=true"))
                .Create();

            await producer.Send("hej med dig");
        }
    }
}