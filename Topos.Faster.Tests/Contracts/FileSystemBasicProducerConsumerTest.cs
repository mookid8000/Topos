using NUnit.Framework;
using Serilog;
using Topos.Tests.Contracts.Broker;

namespace Topos.Faster.Tests.Contracts;

[TestFixture]
public class FileSystemBasicProducerConsumerTest : BasicProducerConsumerTest<FasterLogBrokerFactory>
{
    public FileSystemBasicProducerConsumerTest()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
    }
}