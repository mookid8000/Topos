using System.Threading;
using Topos.Config;
using Topos.InMem;
using Topos.Tests.Contracts.Factories;

namespace Topos.Tests.InMem;

public class InMemBrokerFactory : IBrokerFactory
{
    readonly InMemEventBroker _broker = new InMemEventBroker();

    int _topicCounter;

    public string GetNewTopic()
    {
        var number = Interlocked.Increment(ref _topicCounter);

        return $"topic-{number}";
    }

    public ToposProducerConfigurer ConfigureProducer()
    {
        return Configure
            .Producer(c => c.UseInMemory(_broker))
            .Logging(l => l.UseSerilog());
    }

    public ToposConsumerConfigurer ConfigureConsumer(string groupName)
    {
        return Configure
            .Consumer(groupName, c => c.UseInMemory(_broker))
            .Logging(l => l.UseSerilog());
    }

    public void Dispose()
    {
        throw new System.NotImplementedException();
    }
}