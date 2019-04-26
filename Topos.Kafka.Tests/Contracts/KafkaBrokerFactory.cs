using Serilog;
using Topos.Config;
using Topos.Tests.Contracts;

namespace Topos.Kafka.Tests.Contracts
{
    public class KafkaBrokerFactory : DisposableFactory, IBrokerFactory
    {
        public ToposProducerConfigurer ConfigureProducer()
        {
            return Configure
                .Producer(c => c.UseKafka(KafkaTestConfig.Address))
                .Logging(l => l.UseSerilog());
        }

        public ToposConsumerConfigurer ConfigureConsumer(string groupName)
        {
            return Configure
                .Consumer(groupName, c => c.UseKafka(KafkaTestConfig.Address))
                .Logging(l => l.UseSerilog());
        }

        public string GetNewTopic() => KafkaFixtureBase.GetTopic(Log.Logger);
    }
}