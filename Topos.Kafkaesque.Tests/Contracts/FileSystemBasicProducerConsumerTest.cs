using NUnit.Framework;
using Serilog;
using Topos.Tests.Contracts.Broker;

namespace Topos.Kafkaesque.Tests.Contracts
{
    [TestFixture]
    public class FileSystemBasicProducerConsumerTest : BasicProducerConsumerTest<FileSystemBrokerFactory>
    {
        public FileSystemBasicProducerConsumerTest()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}