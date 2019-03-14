using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Topos.Logging;
using Topos.Logging.Null;
using Topos.Serialization;

namespace Topos.Config
{
    public class MessageHandler : IDisposable
    {
        const int MaxQueueLength = 10000;

        readonly ConcurrentQueue<LogicalMessage> _messages = new ConcurrentQueue<LogicalMessage>();
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        readonly ManualResetEvent _messageHandlerStopped = new ManualResetEvent(false);
        readonly Func<IReadOnlyCollection<LogicalMessage>, CancellationToken, Task> _callback;

        ILogger _logger = new NullLogger();

        bool _disposed;
        bool _stopped;

        public MessageHandler(Func<IReadOnlyCollection<LogicalMessage>, CancellationToken, Task> callback)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        public bool IsReadyForMore => _messages.Count < MaxQueueLength;

        public void Enqueue(LogicalMessage message) => _messages.Enqueue(message);

        public void Start(ILogger logger)
        {
            _logger = logger;

            Task.Run(ProcessMessages);
        }

        public void Stop()
        {
            if (_cancellationTokenSource.IsCancellationRequested) return;

            _logger.Info("Stopping message handler");

            _cancellationTokenSource.Cancel();
        }

        async Task ProcessMessages()
        {
            var cancellationToken = _cancellationTokenSource.Token;

            _logger.Info("Starting message handler");

            try
            {
                var messages = new List<LogicalMessage>(MaxQueueLength);

                while (!cancellationToken.IsCancellationRequested)
                {
                    while (_messages.TryDequeue(out var message))
                    {
                        messages.Add(message);
                    }

                    if (messages.Any())
                    {
                        try
                        {
                            await _callback(messages, cancellationToken);
                        }
                        catch (Exception exception)
                        {
                            _logger.Warn(exception, "Error when handling messages");
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // we're exiting
                _logger.Info("Message handler stopped");
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Unhandled message handler exception");
            }
            finally
            {
                _messageHandlerStopped.Set();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                using (_cancellationTokenSource)
                {
                    Stop();

                    if (!_messageHandlerStopped.WaitOne(TimeSpan.FromSeconds(5)))
                    {
                        _logger.Warn("Message handler did not stop within 5 s timeout");
                    }
                }
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}