using NUnit.Framework;
using Topos.Tests.Contracts.Broker;

namespace Topos.Kafka.Tests.Contracts
{
    [TestFixture]
    public class KafkaBasicProducerConsumerTest : BasicProducerConsumerTest<KafkaBrokerFactory>
    {
    }
}