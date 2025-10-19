using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Polly;
using Topos.Extensions;
using Topos.Logging;
using Topos.Serialization;
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable RedundantAnonymousTypePropertyName
// ReSharper disable InconsistentlySynchronizedField

namespace Topos.Consumer;

public class DefaultConsumerDispatcher : IConsumerDispatcher, IInitializable, IDisposable
{
    readonly ConcurrentDictionary<string, ConcurrentDictionary<int, long>> _previouslySetPositions = new();
    readonly CancellationTokenSource _cancellationTokenSource = new();
    readonly AsyncLock _setPositionsLock = new();
    readonly IMessageSerializer _messageSerializer;
    readonly IPositionManager _positionManager;
    readonly ConsumerContext _consumerContext;
    readonly ILoggerFactory _loggerFactory;
    readonly MessageHandler[] _handlers;
    readonly Policy _dispatchPolicy;
    readonly AsyncPolicy _flushPolicy;
    readonly ILogger _logger;

    bool _disposed;
    Task _flusherLoopTask;

    public DefaultConsumerDispatcher(ILoggerFactory loggerFactory, IMessageSerializer messageSerializer, Handlers handlers, IPositionManager positionManager, ConsumerContext consumerContext)
    {
        if (handlers == null) throw new ArgumentNullException(nameof(handlers));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _messageSerializer = messageSerializer ?? throw new ArgumentNullException(nameof(messageSerializer));
        _positionManager = positionManager ?? throw new ArgumentNullException(nameof(positionManager));
        _consumerContext = consumerContext ?? throw new ArgumentNullException(nameof(consumerContext));
        _handlers = handlers.ToArray();
        _logger = loggerFactory.GetLogger(typeof(DefaultConsumerDispatcher));

        _dispatchPolicy = Policy
            .Handle<Exception>(exception => !(exception is OperationCanceledException && _cancellationTokenSource.IsCancellationRequested)) //< let these exceptions bubble out when we're shutting down
            .WaitAndRetryForever(_ => TimeSpan.FromSeconds(30), (exception, delay) => _logger.Error(exception, "Error when dispatching message - waiting {delay} before trying again", delay));

        _flushPolicy = Policy
            .Handle<Exception>(exception => !(exception is OperationCanceledException && _cancellationTokenSource.IsCancellationRequested)) //< let these exceptions bubble out when we're shutting down
            .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(30), (exception, delay) => _logger.Error(exception, "Error when flushing positions - waiting {delay} before trying again", delay));
    }

    public void Initialize()
    {
        foreach (var handler in _handlers)
        {
            var handlerType = handler.GetType();
            var logger = _loggerFactory.GetLogger(handlerType);

            handler.Start(logger, _consumerContext);
        }

        _flusherLoopTask = Task.Run(RunPositionsFlusherAsync);
    }

    public void Dispatch(ReceivedTransportMessage transportMessage)
    {
        _dispatchPolicy.Execute(token =>
        {
            var receivedLogicalMessage = _messageSerializer.Deserialize(transportMessage);

            for (var index = 0; index < _handlers.Length; index++)
            {
                var handler = _handlers[index];

                while (!handler.IsReadyForMore)
                {
                    Task.Delay(TimeSpan.FromSeconds(1), token).WaitSafe(token);
                }

                handler.Enqueue(receivedLogicalMessage);
            }
        }, _cancellationTokenSource.Token);
    }

    public async Task RevokeAsync(string topic, IEnumerable<int> partitions)
    {
        var partitionsList = partitions.ToList();

        var token = _cancellationTokenSource.Token;

        using (await _setPositionsLock.LockAsync(cancellationToken: token))
        {
            try
            {
                // ensure all handlers have finished processing their queues
                foreach (var handler in _handlers)
                {
                    await handler.DrainAsync(token);
                }

                // save all positions
                await SetPositionsAsync();

                // remove cached positions from handlers
                foreach (var handler in _handlers)
                {
                    handler.Clear(topic, partitionsList);
                }

                if (_previouslySetPositions.TryGetValue(topic, out var positions))
                {
                    foreach (var partition in partitionsList)
                    {
                        positions.TryRemove(partition, out _);
                    }
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                // it's ok, we're shutting down
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Error when flushing positions in revoke callback");
            }
        }
    }

    async Task RunPositionsFlusherAsync()
    {
        async Task DoFlush(CancellationToken cancellationToken)
        {
            using (await _setPositionsLock.LockAsync(cancellationToken))
            {
                await SetPositionsAsync();
            }
        }

        var token = _cancellationTokenSource.Token;

        try
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), token);

                await _flushPolicy.ExecuteAsync(DoFlush, token);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // it's fine
        }
        catch (Exception exception)
        {
            _logger.Error(exception, "Error in positions flusher");
        }
        finally
        {
            // set the positions one last time
            try
            {
                await SetPositionsAsync();
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch { }
        }
    }

    async Task SetPositionsAsync()
    {
        ConcurrentDictionary<int, long> GetForTopic(string topic) => _previouslySetPositions.GetOrAdd(topic, _ => new ConcurrentDictionary<int, long>());

        var positions = _handlers
            .SelectMany(h => h.GetPositions())
            .GroupBy(p => new { p.Topic, p.Partition })
            .Select(p => new Position(p.Key.Topic, p.Key.Partition, p.Min(a => a.Offset)))
            .Where(p =>
            {
                var currentOffset = GetForTopic(p.Topic).TryGetValue(p.Partition, out var result)
                    ? result
                    : -1;

                return currentOffset < p.Offset;
            })
            .ToList();

        if (!positions.Any()) return;

        _logger.Debug("Setting positions {@positions}", positions.Select(p => new { Topic = p.Topic, Partition = p.Partition, Offset = p.Offset }));

        await Task.WhenAll(positions.Select(async position =>
        {
            await _positionManager.SetAsync(position);

            GetForTopic(position.Topic)[position.Partition] = position.Offset;
        }));
    }

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            foreach (var handler in _handlers)
            {
                handler.Stop();
            }

            _cancellationTokenSource.Cancel();

            foreach (var handler in _handlers)
            {
                handler.Dispose();
            }

            if (_flusherLoopTask != null)
            {
                if (!_flusherLoopTask.WaitSafe(TimeSpan.FromSeconds(3)))
                {
                    _logger.Warn("Positions flusher loop did not exit/finish committing the last position within 3 s timeout - positions may not have been properly committed");
                }
            }
        }
        finally
        {
            _disposed = true;
        }
    }
}