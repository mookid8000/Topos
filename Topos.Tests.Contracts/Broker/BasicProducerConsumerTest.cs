using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Topos.Config;

namespace Topos.Tests.Contracts.Broker
{
    public abstract class BasicProducerConsumerTest<TProducerFactory> : ToposFixtureBase where TProducerFactory : IBrokerFactory, new()
    {
        IBrokerFactory _brokerFactory;

        [Test]
        public async Task CanStartProducer()
        {
            var producer = BrokerFactory.ConfigureProducer()
                .Create();

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        IBrokerFactory BrokerFactory
        {
            get
            {
                if (_brokerFactory != null) return _brokerFactory;
                _brokerFactory = new TProducerFactory();
                Using(_brokerFactory);
                return _brokerFactory;
            }
        }
    }
}