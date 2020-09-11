using NUnit.Framework;
using Serilog;
using Topos.Tests.Contracts.Broker;

namespace Topos.Faster.Tests.Contracts
{
    [TestFixture]
    public class FileSystemBatchProducerTest : BatchProducerTest<FasterLogBrokerFactory>
    {
        public FileSystemBatchProducerTest()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}