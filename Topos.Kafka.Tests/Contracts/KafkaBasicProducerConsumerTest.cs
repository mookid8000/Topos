using NUnit.Framework;
using Serilog;
using Topos.Tests.Contracts.Broker;

namespace Topos.Kafka.Tests.Contracts;

[TestFixture]
public class KafkaBasicProducerConsumerTest : BasicProducerConsumerTest<KafkaBrokerFactory>
{
    public KafkaBasicProducerConsumerTest()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
    }
}