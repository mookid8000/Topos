using System.Threading.Tasks;
using Newtonsoft.Json;
using Topos.Internals;
using Topos.Logging;
using Topos.Serialization;

namespace Topos.Kafkaesque
{
    class FileSystemProducerImplementation : IProducerImplementation
    {
        readonly FileEventBuffer _fileEventBuffer;

        public FileSystemProducerImplementation(string directoryPath, ILoggerFactory loggerFactory)
        {
            _fileEventBuffer = new FileEventBuffer(directoryPath, loggerFactory);
        }

        public async Task Send(string topic, string partitionKey, TransportMessage transportMessage)
        {
            var text = JsonConvert.SerializeObject(transportMessage);
          
            _fileEventBuffer.Append(new[] {text});
        }

        public void Dispose()
        {
            _fileEventBuffer.Dispose();
        }
    }
}