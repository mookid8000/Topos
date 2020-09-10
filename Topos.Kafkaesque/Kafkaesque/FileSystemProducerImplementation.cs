using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kafkaesque;
using Newtonsoft.Json;
using Topos.Internals;
using Topos.Logging;
using Topos.Serialization;
using ILogger = Topos.Logging.ILogger;

namespace Topos.Kafkaesque
{
    class FileSystemProducerImplementation : IProducerImplementation
    {
        readonly ConcurrentDictionary<string, Lazy<LogWriter>> _writers = new ConcurrentDictionary<string, Lazy<LogWriter>>();
        readonly string _directoryPath;
        readonly ILogger _logger;

        bool _disposed;

        public FileSystemProducerImplementation(string directoryPath, ILoggerFactory loggerFactory)
        {
            _directoryPath = directoryPath;
            _logger = loggerFactory.GetLogger(typeof(FileSystemProducerImplementation));
            _logger.Info("Kafkaesque producer initialized with directory {directoryPath}", directoryPath);
        }

        public async Task Send(string topic, string partitionKey, TransportMessage transportMessage)
        {
            // partitionKey has no function with Kafkaesque-based broker
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            if (transportMessage == null) throw new ArgumentNullException(nameof(transportMessage));

            var eventData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(transportMessage));
            var writer = _writers.GetOrAdd(topic, CreateWriter).Value;

            await writer.WriteAsync(eventData);
        }

        public async Task SendMany(string topic, string partitionKey, IEnumerable<TransportMessage> transportMessages)
        {
            // partitionKey has no function with Kafkaesque-based broker
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            if (transportMessages == null) throw new ArgumentNullException(nameof(transportMessages));
            
            var writer = _writers.GetOrAdd(topic, CreateWriter).Value;

            await writer.WriteManyAsync(transportMessages
                .Select(transportMessage => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(transportMessage))));
        }

        Lazy<LogWriter> CreateWriter(string topic)
        {
            var topicDirectoryPath = Path.Combine(_directoryPath, topic);

            return new Lazy<LogWriter>(() =>
            {
                var logDirectory = new LogDirectory(topicDirectoryPath, new Settings(logger: new KafkaesqueToToposLogger(_logger)));

                _logger.Debug("Initializing new Kafkaesque writer with path {directoryPath}", topicDirectoryPath);

                return logDirectory.GetWriter();
            });
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                var writers = _writers.Values;

                _logger.Info("Closing {count} Kafkaesque writers", writers.Count);

                Parallel.ForEach(writers, writer => writer.Value.Dispose());

                _logger.Info("Kafkaesque writers successfully closed");
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}