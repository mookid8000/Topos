using Topos.Config;
using Topos.Tests.Contracts;

namespace Topos.Kafka.Tests.Contracts
{
    public class KafkaBrokerFactory : DisposableFactory, IBrokerFactory
    {
        public ToposProducerConfigurer ConfigureProducer()
        {
            return null;
        }
    }
}