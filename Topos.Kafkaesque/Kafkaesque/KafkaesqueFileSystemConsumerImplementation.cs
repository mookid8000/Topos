using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kafkaesque;
using Newtonsoft.Json;
using Topos.Consumer;
using Topos.Internals;
using Topos.Logging;
using Topos.Serialization;
using ILogger = Topos.Logging.ILogger;
// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleLiteral

namespace Topos.Kafkaesque
{
    class KafkaesqueFileSystemConsumerImplementation : IConsumerImplementation, IDisposable
    {
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        readonly IConsumerDispatcher _consumerDispatcher;
        readonly IPositionManager _positionManager;
        readonly List<Thread> _workers;
        readonly string _directoryPath;
        readonly ILogger _logger;

        bool _disposed;

        public KafkaesqueFileSystemConsumerImplementation(string directoryPath, ILoggerFactory loggerFactory, IEnumerable<string> topics, string group, IConsumerDispatcher consumerDispatcher, IPositionManager positionManager)
        {
            if (topics == null) throw new ArgumentNullException(nameof(topics));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _directoryPath = directoryPath;
            _consumerDispatcher = consumerDispatcher ?? throw new ArgumentNullException(nameof(consumerDispatcher));
            _positionManager = positionManager ?? throw new ArgumentNullException(nameof(positionManager));

            _logger = loggerFactory.GetLogger(typeof(KafkaesqueFileSystemConsumerImplementation));

            _workers = topics
                .Select(topic => new Thread(() => PumpTopic(topic)) { Name = $"Kafkaesque worker for topic '{topic}'" })
                .ToList();
        }

        public void Start() => _workers.ForEach(t => t.Start());

        void PumpTopic(string topic)
        {
            var cancellationToken = _cancellationTokenSource.Token;

            _logger.Info("Starting consumer worker for topic {topic}", topic);

            try
            {
                var topicDirectoryPath = Path.Combine(_directoryPath, topic);
                var logDirectory = new LogDirectory(topicDirectoryPath, new Settings(logger: new KafkaesqueToToposLogger(_logger)));
                var reader = logDirectory.GetReader();

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var resumePosition = _positionManager.Get(topic, 0).Result;

                        var (fileNumber, bytePosition) = resumePosition.ToKafkaesquePosition();

                        _logger.Debug("Resuming consumer from file {fileNumber} byte {bytePosition}", fileNumber, bytePosition);

                        foreach (var eventData in reader.Read(fileNumber, bytePosition, cancellationToken: cancellationToken))
                        {
                            var transportMessage = JsonConvert.DeserializeObject<TransportMessage>(Encoding.UTF8.GetString(eventData.Data));
                            var kafkaesqueEventPosition = new KafkaesquePosition(eventData.FileNumber, eventData.BytePosition);
                            var eventPosition = kafkaesqueEventPosition.ToPosition(topic, partition: 0);
                            var receivedTransportMessage = new ReceivedTransportMessage(eventPosition, transportMessage.Headers, transportMessage.Body);

                            _logger.Debug("Received event {position}", eventPosition);

                            _consumerDispatcher.Dispatch(receivedTransportMessage);
                        }
                    }
                    catch (Exception exception)
                    {
                        _logger.Warn(exception, "Error in consumer worker for topic {topic} - waiting 10 s", topic);
                        
                        Task.Delay(TimeSpan.FromSeconds(10), cancellationToken)
                            .Wait(cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // we're done
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Unhandled exception in consumer worker for topic {topic}", topic);
            }
            finally
            {
                _logger.Info("Stopped consumer worker for topic {topic}", topic);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _cancellationTokenSource.Cancel();

                foreach (var worker in _workers)
                {
                    if (worker.ThreadState == ThreadState.Running)
                    {
                        if (!worker.Join(TimeSpan.FromSeconds(3)))
                        {
                            _logger.Warn("Worker loop named {workerName} did not exit within 3 s timeout!", worker.Name);
                        }
                    }
                }
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _disposed = true;
            }
        }
    }
}