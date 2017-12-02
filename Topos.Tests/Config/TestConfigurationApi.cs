using NUnit.Framework;
using Topos.Config;
using Topos.SqlServer.Config;

namespace Topos.Tests.Config
{
    [TestFixture]
    public class TestConfigurationApi : FixtureBase
    {
        [Test]
        public void LooksGoodMan()
        {
            Configure.Topos()
                .Transport(t => t.UseSqlServer("server=.; database=topos_test; trusted_connection=true"))
                .Start();
        }
    }
}