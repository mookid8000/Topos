using NUnit.Framework;
using Topos.Config;
using Topos.Tests.Contracts;
using Topos.Tests.Contracts.Broker;

namespace Topos.Tests.FileSystem
{
    [TestFixture]
    public class FileSystemBasicProducerConsumerTest : BasicProducerConsumerTest<FileSystemBrokerFactory>
    {
        
    }

    public class FileSystemBrokerFactory : IBrokerFactory
    {
        public ToposProducerConfigurer ConfigureProducer()
        {
            throw new System.NotImplementedException();
        }

        public ToposConsumerConfigurer ConfigureConsumer(string groupName)
        {
            throw new System.NotImplementedException();
        }

        public string GetTopic()
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}