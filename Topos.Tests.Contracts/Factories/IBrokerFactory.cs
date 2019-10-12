using System;
using Topos.Config;

namespace Topos.Tests.Contracts.Factories
{
    public interface IBrokerFactory : IDisposable
    {
        ToposProducerConfigurer ConfigureProducer();
        ToposConsumerConfigurer ConfigureConsumer(string groupName);
        string GetNewTopic();
    }
}