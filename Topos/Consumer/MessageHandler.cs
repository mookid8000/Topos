using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Topos.Config;
using Topos.Extensions;
using Topos.Helpers;
using Topos.Logging;
using Topos.Logging.Null;
using Topos.Serialization;
// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleAnonymousFunction

namespace Topos.Consumer;

public class MessageHandler : IDisposable
{
    public const string MinimumBatchSizeOptionsKey = "message-handler-minimum-batch-size";
    public const string MaximumBatchSizeOptionsKey = "message-handler-maximum-batch-size";
    public const string MaximumPrefetchQueueLengthOptionsKey = "message-handler-maximum-prefecth-queue-length";

    const int DefaultMinimumBatchSize = 1;
    const int DefaultMaximumBatchSize = 1000;

    const int DefaultMaxPrefetchQueueLength = 100000;

    readonly ConcurrentDictionary<string, ConcurrentDictionary<int, long>> _positions = new();
    readonly ConcurrentQueue<ReceivedLogicalMessage> _messages = new();
    readonly AsyncSemaphore _messagesSemaphore = new(initialCount: 0, maxCount: int.MaxValue);
    readonly CancellationTokenSource _cancellationTokenSource = new();

    readonly MessageHandlerDelegate _callback;
    readonly AsyncRetryPolicy _callbackPolicy;
    readonly Options _options;

    ILogger _logger = new NullLogger();

    int _minimumBatchSize;
    int _maximumBatchSize;
    int _maxPrefetchQueueLength;
    Task _task;
    bool _disposed;
    ConsumerContext _context;

    public MessageHandler(MessageHandlerDelegate callback, Options options)
    {
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        _options = options ?? throw new ArgumentNullException(nameof(options));

        _callbackPolicy = Policy
            .Handle<Exception>(exception => !(exception is OperationCanceledException && _cancellationTokenSource.IsCancellationRequested)) //< let these exceptions bubble out when we're shutting down
            .WaitAndRetryForeverAsync(i => TimeSpan.FromSeconds(Math.Min(60, i * 2)), LogException);
    }

    public bool IsReadyForMore => _messages.Count < _maxPrefetchQueueLength;

    public void Enqueue(ReceivedLogicalMessage receivedLogicalMessage)
    {
        _messages.Enqueue(receivedLogicalMessage);
        _messagesSemaphore.Increment();
    }

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

    async Task ProcessMessages()
    {
        var cancellationToken = _cancellationTokenSource.Token;

        _logger.Info("Starting message handler");

        try
        {
            var messageBatch = new List<ReceivedLogicalMessage>(_maximumBatchSize);

            while (!cancellationToken.IsCancellationRequested)
            {
                await _messagesSemaphore.DecrementAsync(cancellationToken);

                // while we still have room for messages
                while (messageBatch.Count < _maximumBatchSize)
                {
                    // break out if the queue is empty
                    if (!_messages.TryDequeue(out var message)) break;

                    messageBatch.Add(message);
                }

                // if we're under the minimum batch size, wait a short while before trying again
                if (messageBatch.Count < _minimumBatchSize)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
                    continue;
                }

                // dispatch message batch to handlers
                await _callbackPolicy.ExecuteAsync(token => _callback(messageBatch, _context, token), cancellationToken);

                var maxPositions = messageBatch
                    .GroupBy(m => new { m.Position.Topic, m.Position.Partition })
                    .Select(a => new Position(a.Key.Topic, a.Key.Partition, a.Max(p => p.Position.Offset)))
                    .ToList();

                foreach (var position in maxPositions)
                {
                    var topicPositions = _positions.GetOrAdd(position.Topic, _ => new ConcurrentDictionary<int, long>());

                    topicPositions.AddOrUpdate(
                        key: position.Partition,
                        addValue: position.Offset,
                        updateValueFactory: (_, currentOffset) => Math.Max(position.Offset, currentOffset) //< ensure we'll never downwrite an offset
                    );
                }

                messageBatch.Clear();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // it's ok, we're shutting down
        }
        catch (Exception exception)
        {
            _logger.Error(exception, "Unhandled message handler exception");
        }

        _logger.Info("Message handler stopped");
    }

    public async Task Drain(CancellationToken token)
    {
        // wait until queue is empty
        while (!token.IsCancellationRequested && _messages.Count > 0)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(200), token);
        }

        // wait a little while extra
        await Task.Delay(TimeSpan.FromMilliseconds(250), token);
    }

    public void Clear(string topic, IEnumerable<int> partitionsList)
    {
        if (!_positions.TryGetValue(topic, out var positions)) return;

        foreach (var partition in partitionsList)
        {
            positions.TryRemove(partition, out _);
        }
    }

    public IEnumerable<Position> GetPositions() => _positions
        .SelectMany(topic => topic.Value.Select(partition => new Position(topic.Key, partition.Key, partition.Value)));

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            using (_cancellationTokenSource)
            using (_messagesSemaphore)
            {
                Stop();

                if (!_task.WaitSafe(TimeSpan.FromSeconds(5)))
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
}