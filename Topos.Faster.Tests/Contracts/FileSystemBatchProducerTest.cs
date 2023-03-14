using NUnit.Framework;
using Serilog;
using Topos.Faster.Tests.Contracts.Factories;
using Topos.Tests.Contracts.Broker;

namespace Topos.Faster.Tests.Contracts;

[TestFixture]
public class FileSystemBatchProducerTest : BatchProducerTest<FasterLogFileSystemBrokerFactory>
{
    public FileSystemBatchProducerTest()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
    }
}