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
        public void CanConfigure_Consumer()
        {
            Configure.Consumer()
                .Transport(t => t.UseSqlServer("server=.; database=topos_test; trusted_connection=true"))
                .Start();
        }

        [Test]
        public async Task CanConfigure_Producer()
        {
            var producer = Configure.Producer()
                .Transport(t => t.UseSqlServer("server=.; database=topos_test; trusted_connection=true"))
                .Create();


        }
    }
}