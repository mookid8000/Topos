using NUnit.Framework;
using Testy;
using Topos.Internals;

namespace Topos.Kafka.Tests;

[TestFixture]
public class TestConnectionStringMutator : FixtureBase
{
    [Test]
    public void CanParseConnectionString()
    {
        var parser = new ConnectionStringParser("Endpoint=sb://ongo-bongo.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=naaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaw=");

        Assert.That(parser.HasElement("Endpoint"), Is.True);
        Assert.That(parser.HasElement("endpoint"), Is.True);
        Assert.That(parser.GetValue("endpoint"), Is.EqualTo("sb://ongo-bongo.servicebus.windows.net/"));
    }

    [Test]
    public void TestEventHubsHelper()
    {
        string bootstrapServers = null;
        string username = null;
        string password = null;

        AzureEventHubsHelper.TrySetConnectionInfo("Endpoint=sb://ongo-bongo.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=naaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaw=",
            info =>
            {
                bootstrapServers = info.BootstrapServers;
                username = info.SaslUsername;
                password = info.SaslPassword;
            });

        Assert.That(bootstrapServers, Is.EqualTo("ongo-bongo.servicebus.windows.net:9093"));
        Assert.That(username, Is.EqualTo("$ConnectionString"));
        Assert.That(password, Is.EqualTo("Endpoint=sb://ongo-bongo.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=naaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaw="));
    }
}