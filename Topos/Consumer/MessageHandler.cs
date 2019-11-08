using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Topos.Config;
using Topos.Logging;
using Topos.Logging.Null;
using Topos.Serialization;
// ReSharper disable ArgumentsStyleNamedExpression

namespace Topos.Consumer
{
    public class MessageHandler : IDisposable
    {
        public const string MinimumBatchSizeOptionsKey = "message-handler-minimum-batch-size";
        public const string MaximumBatchSizeOptionsKey = "message-handler-maximum-batch-size";
        public const string MaximumPrefetchQueueLengthOptionsKey = "message-handler-maximum-prefecth-queue-length";

        const int DefaultMinimumBatchSize = 1;
        const int DefaultMaximumBatchSize = 1000;

        const int DefaultMaxPrefetchQueueLength = 20000;

        readonly ConcurrentDictionary<string, ConcurrentDictionary<int, long>> _positions = new ConcurrentDictionary<string, ConcurrentDictionary<int, long>>();
        readonly ConcurrentQueue<ReceivedLogicalMessage> _messages = new ConcurrentQueue<ReceivedLogicalMessage>();
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        readonly Func<IReadOnlyCollection<ReceivedLogicalMessage>, ConsumerContext, CancellationToken, Task> _callback;
        readonly AsyncRetryPolicy _callbackPolicy;
        readonly Options _options;

        ILogger _logger = new NullLogger();

        int _minimumBatchSize;
        int _maximumBatchSize;
        int _maxPrefetchQueueLength;
        Task _task;
        bool _disposed;
        ConsumerContext _context;

        public MessageHandler(Func<IReadOnlyCollection<ReceivedLogicalMessage>, ConsumerContext, CancellationToken, Task> callback, Options options)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _callbackPolicy = Policy.Handle<Exception>().WaitAndRetryForeverAsync(i => TimeSpan.FromSeconds(i * 2), LogException);
        }

        void LogException(Exception exception, TimeSpan delay)
        {
            if (delay < TimeSpan.FromSeconds(10))
            {
                _logger.Warn(exception, "Exception when executing message handler - waiting {delay} before trying again", delay);
            }
            else
            {
                _logger.Error(exception, "Exception when executing message handler - waiting {delay} before trying again", delay);
            }
        }

        public bool IsReadyForMore => _messages.Count < _maxPrefetchQueueLength;

        public void Enqueue(ReceivedLogicalMessage receivedLogicalMessage) => _messages.Enqueue(receivedLogicalMessage);

        public void Start(ILogger logger, ConsumerContext context)
        {
            _minimumBatchSize = _options.Get(MinimumBatchSizeOptionsKey, defaultValue: DefaultMinimumBatchSize);
            _maximumBatchSize = _options.Get(MaximumBatchSizeOptionsKey, defaultValue: DefaultMaximumBatchSize);
            _maxPrefetchQueueLength = _options.Get(MaximumPrefetchQueueLengthOptionsKey, defaultValue: DefaultMaxPrefetchQueueLength);

            if (_minimumBatchSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(_minimumBatchSize), _minimumBatchSize, "Please set a MIN batch size >= 1");
            }

            if (_maximumBatchSize < _minimumBatchSize)
            {
                throw new ArgumentOutOfRangeException(nameof(_maximumBatchSize), _maximumBatchSize, $"MAX batch size must be >= {_minimumBatchSize} (which is the current MIN batch size)");
            }

            if (_minimumBatchSize > _maxPrefetchQueueLength)
            {
                throw new ArgumentOutOfRangeException(nameof(_minimumBatchSize), _minimumBatchSize, $"MIN batch size must be <= {_maxPrefetchQueueLength} (which is the current MAX prefetch queue length)");
            }

            _context = context;
            _logger = logger;
            _task = Task.Run(ProcessMessages);
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
                var messageBatch = new List<ReceivedLogicalMessage>(_maximumBatchSize);

                while (!cancellationToken.IsCancellationRequested)
                {
                    // IMPORTANT: The check for count must be first! Otherwise, we might pop a message and not add it to the batch... :|
                    while (messageBatch.Count < _maximumBatchSize && _messages.TryDequeue(out var message))
                    {
                        messageBatch.Add(message);
                    }

                    if (messageBatch.Count < _minimumBatchSize)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
                        continue;
                    }

                    try
                    {
                        await _callbackPolicy.ExecuteAsync(token => _callback(messageBatch, _context, token), cancellationToken);

                        var maxPositionByPartition = messageBatch
                            .GroupBy(m => new { m.Position.Topic, m.Position.Partition })
                            .Select(a => new
                            {
                                a.Key.Topic,
                                a.Key.Partition,
                                Offset = a.Max(p => p.Position.Offset)
                            })
                            .ToList();

                        foreach (var max in maxPositionByPartition)
                        {
                            _positions.GetOrAdd(max.Topic, _ => new ConcurrentDictionary<int, long>())[max.Partition] = max.Offset;
                        }

                        messageBatch.Clear();
                    }
                    catch (Exception exception)
                    {
                        _logger.Error(exception, "Critical error when handling messages");
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
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                using (_cancellationTokenSource)
                {
                    Stop();

                    if (!_task.Wait(TimeSpan.FromSeconds(5)))
                    {
                        _logger.Warn("Message handler worker task did not stop within 5 s timeout");
                    }
                }
            }
            finally
            {
                _disposed = true;
            }
        }

        public IEnumerable<Position> GetPositions()
        {
            return _positions
                .SelectMany(topic => topic.Value
                    .Select(partition => new Position(topic.Key, partition.Key, partition.Value)));
        }
    }
}