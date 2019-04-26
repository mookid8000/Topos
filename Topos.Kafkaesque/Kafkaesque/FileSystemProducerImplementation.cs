using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Kafkaesque;
using Newtonsoft.Json;
using Topos.Logging;
using Topos.Serialization;

namespace Topos.Kafkaesque
{
    class FileSystemProducerImplementation : IProducerImplementation
    {
        readonly ConcurrentDictionary<string, LogWriter> _writers = new ConcurrentDictionary<string, LogWriter>();
        readonly string _directoryPath;
        readonly ILogger _logger;

        public FileSystemProducerImplementation(string directoryPath, ILoggerFactory loggerFactory)
        {
            _directoryPath = directoryPath;
            _logger = loggerFactory.GetLogger(typeof(FileSystemProducerImplementation));
        }

        public async Task Send(string topic, string partitionKey, TransportMessage transportMessage)
        {
            var eventData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(transportMessage));
            var writer = _writers.GetOrAdd(topic, CreateWriter);
            
            await writer.WriteAsync(eventData);
        }

        LogWriter CreateWriter(string topic)
        {
            var topicDirectoryPath = Path.Combine(_directoryPath, topic);
            var logDirectory = new LogDirectory(topicDirectoryPath);

            return logDirectory.GetWriter();
        }

        public void Dispose() => Parallel.ForEach(_writers.Values, writer => writer.Dispose());
    }
}