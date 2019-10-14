using NUnit.Framework;
using Serilog;
using Topos.Tests.Contracts.Broker;

namespace Topos.Kafka.Tests.Contracts
{
    [TestFixture]
    public class KafkaMaxQueueLengthCustomizationTest : MaxQueueLengthCustomizationTest<KafkaBrokerFactory>
    {
        public KafkaMaxQueueLengthCustomizationTest()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}