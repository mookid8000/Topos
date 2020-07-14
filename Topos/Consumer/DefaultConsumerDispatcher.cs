using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Topos.Logging;
using Topos.Serialization;
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable RedundantAnonymousTypePropertyName

namespace Topos.Consumer
{
    public class DefaultConsumerDispatcher : IConsumerDispatcher, IInitializable, IDisposable
    {
        readonly ConcurrentDictionary<string, ConcurrentDictionary<int, long>> _previouslySetPositions = new ConcurrentDictionary<string, ConcurrentDictionary<int, long>>();
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        readonly IMessageSerializer _messageSerializer;
        readonly IPositionManager _positionManager;
        readonly ConsumerContext _consumerContext;
        readonly ILoggerFactory _loggerFactory;
        readonly MessageHandler[] _handlers;
        readonly Policy _dispatchPolicy;
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
        }

        public void Initialize()
        {
            foreach (var handler in _handlers)
            {
                var handlerType = handler.GetType();
                var logger = _loggerFactory.GetLogger(handlerType);

                handler.Start(logger, _consumerContext);
            }

            _flusherLoopTask = Task.Run(async () => await RunPositionsFlusher());
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
                        Task.Delay(TimeSpan.FromSeconds(1), token).Wait(token);
                    }

                    handler.Enqueue(receivedLogicalMessage);
                }
            }, _cancellationTokenSource.Token);
        }

        public async Task Flush(string topic, IEnumerable<int> partitions)
        {
        }

        async Task RunPositionsFlusher()
        {
            var token = _cancellationTokenSource.Token;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), token);

                    await SetPositions();
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
                    await SetPositions();
                }
                catch { }
            }
        }

        async Task SetPositions()
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
                await _positionManager.Set(position);

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
                    if (!_flusherLoopTask.Wait(TimeSpan.FromSeconds(3)))
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

}