using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Topos.Consumer;
using Topos.Internals;
using Topos.Logging;
using Topos.Serialization;

namespace Topos.Kafkaesque
{
    class FileSystemConsumerImplementation : IConsumerImplementation, IDisposable
    {
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        readonly ManualResetEvent _exitedWorkerLoop = new ManualResetEvent(false);
        readonly ILoggerFactory _loggerFactory;
        readonly IConsumerDispatcher _consumerDispatcher;
        readonly IPositionManager _positionManager;
        readonly string _directoryPath;
        readonly string[] _topics;
        readonly ILogger _logger;

        bool _disposed;

        public FileSystemConsumerImplementation(string directoryPath, ILoggerFactory loggerFactory, IEnumerable<string> topics, string group,
            IConsumerDispatcher consumerDispatcher, IPositionManager positionManager)
        {
            if (topics == null) throw new ArgumentNullException(nameof(topics));
            _directoryPath = directoryPath;
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _consumerDispatcher = consumerDispatcher ?? throw new ArgumentNullException(nameof(consumerDispatcher));
            _positionManager = positionManager ?? throw new ArgumentNullException(nameof(positionManager));

            _logger = loggerFactory.GetLogger(typeof(FileSystemConsumerImplementation));
            _topics = topics.ToArray();
        }

        public void Start() => Task.Run(Work);

        async Task Work()
        {
            var cancellationToken = _cancellationTokenSource.Token;

            using (var fileBuffer = new FileEventBuffer(_directoryPath, _loggerFactory))
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var readResult = fileBuffer.Read();

                            if (readResult.IsEmpty)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(0.2), cancellationToken);
                                continue;
                            }

                            foreach (var line in readResult)
                            {
                                var transportMessage = JsonConvert.DeserializeObject<TransportMessage>(line);
                                var position = new Position("bim", 0, readResult.Position);
                                var headers = transportMessage.Headers;
                                var body = transportMessage.Body;
                                var receivedTransportMessage = new ReceivedTransportMessage(position, headers, body);

                                _consumerDispatcher.Dispatch(receivedTransportMessage);
                            }
                        }
                        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                        {
                            // exit
                        }
                        catch (Exception exception)
                        {
                            _logger.Error(exception,
                                "An error ocurred when reading file buffer from path {directoryPath}", _directoryPath);
                        }
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // exit
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, "Unhandled exception in file system consumer implementation");
                }
                finally
                {
                    _exitedWorkerLoop.Set();
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _cancellationTokenSource.Cancel();

                if (!_exitedWorkerLoop.WaitOne(TimeSpan.FromSeconds(3)))
                {
                    _logger.Warn("Worker loop did not exit within 3 s timeout!");
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