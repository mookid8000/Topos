using Topos.Producer;
using Topos.Tests.Contracts;

namespace Topos.Kafka.Tests.Contracts
{
    public class KafkaBrokerFactory : DisposableFactory, IBrokerFactory
    {


        public IToposProducer Create()
        {
            return null;
        }
    }
}