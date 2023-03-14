using Testy.Files;
using Topos.Config;
using Topos.Tests.Contracts;
using Topos.Tests.Contracts.Factories;

namespace Topos.Faster.Tests.Contracts.Factories;

public class FasterLogFileSystemBrokerFactory : DisposableFactory, IBrokerFactory
{
    readonly TemporaryTestDirectory _temporaryTestDirectory = new();

    int _counter;

    public FasterLogFileSystemBrokerFactory() => Using(_temporaryTestDirectory);

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