using NUnit.Framework;
using Serilog;
using Testy.Files;
using Topos.Config;
using Topos.Tests.Contracts;
using Topos.Tests.Contracts.Broker;

namespace Topos.Kafkaesque.Tests.Contracts
{
    [TestFixture]
    public class FileSystemBasicProducerConsumerTest : BasicProducerConsumerTest<FileSystemBasicProducerConsumerTest.FileSystemBrokerFactory>
    {
        public FileSystemBasicProducerConsumerTest()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        }

        public class FileSystemBrokerFactory : DisposableFactory, IBrokerFactory
        {
            readonly TemporaryTestDirectory _temporaryTestDirectory = new TemporaryTestDirectory();

            int _counter;

            public FileSystemBrokerFactory() => Using(_temporaryTestDirectory);

            public ToposProducerConfigurer ConfigureProducer() =>
                Configure
                    .Producer(p => p.UseFileSystem(_temporaryTestDirectory.ToString()))
                    .Logging(l => l.UseSerilog());

            public ToposConsumerConfigurer ConfigureConsumer(string groupName) =>
                Configure
                    .Consumer(groupName, c => c.UseFileSystem(_temporaryTestDirectory.ToString()))
                    .Logging(l => l.UseSerilog());

            public string GetNewTopic() => $"topic{_counter++}";
        }
    }
}