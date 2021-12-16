using NUnit.Framework;
using Testy;
using Topos.Config;
using Topos.InMem;

namespace Topos.NewtonsoftJson.Tests;

[TestFixture]
[Ignore("wait with this")]
public class TestConfigurationApi : FixtureBase
{
    [Test]
    public void CanConfigure_Producer()
    {
        var consumer = Configure.Consumer("default-group", t => t.UseInMemory(new InMemEventBroker()))
            .Serialization(s => s.UseNewtonsoftJson())
            .Start();

        Using(consumer);
    }

    [Test]
    public void CanConfigure_Consumer()
    {
        var consumer = Configure.Consumer("default-group", t => t.UseInMemory(new InMemEventBroker()))
            .Serialization(s => s.UseNewtonsoftJson())
            .Start();

        Using(consumer);
    }
}