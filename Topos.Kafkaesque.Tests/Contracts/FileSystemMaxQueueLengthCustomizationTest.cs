using NUnit.Framework;
using Serilog;
using Topos.Tests.Contracts.Broker;

namespace Topos.Kafkaesque.Tests.Contracts
{
    [TestFixture]
    public class FileSystemMaxQueueLengthCustomizationTest : MaxQueueLengthCustomizationTest<FileSystemBrokerFactory>
    {
        public FileSystemMaxQueueLengthCustomizationTest()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}