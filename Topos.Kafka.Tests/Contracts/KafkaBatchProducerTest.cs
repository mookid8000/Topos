using NUnit.Framework;
using Serilog;
using Topos.Tests.Contracts.Broker;

namespace Topos.Kafka.Tests.Contracts;

[TestFixture]
public class KafkaBatchProducerTest : BatchProducerTest<KafkaBrokerFactory>
{
    public KafkaBatchProducerTest()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
    }
}