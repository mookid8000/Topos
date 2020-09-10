using System.Collections.Generic;
using System.Threading.Tasks;
using Topos.Logging;
using Topos.Serialization;

namespace Topos.Faster
{
    class FileSystemProducerImplementation : IProducerImplementation
    {
        public FileSystemProducerImplementation(string directoryPath, ILoggerFactory loggerFactory)
        {
            
        }

        public Task Send(string topic, string partitionKey, TransportMessage transportMessage)
        {
            throw new System.NotImplementedException();
        }

        public Task SendMany(string topic, string partitionKey, IEnumerable<TransportMessage> transportMessages)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}