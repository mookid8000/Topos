using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Topos.Logging;
using Topos.Serialization;
// ReSharper disable ForCanBeConvertedToForeach

namespace Topos.Consumer
{
    public class DefaultConsumerDispatcher : IConsumerDispatcher, IInitializable, IDisposable
    {
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        readonly ILoggerFactory _loggerFactory;
        readonly IMessageSerializer _messageSerializer;
        readonly IPositionManager _positionManager;
        readonly MessageHandler[] _handlers;
        readonly ILogger _logger;

        bool _disposed;
        Task _flusherLoopTask;

        public DefaultConsumerDispatcher(ILoggerFactory loggerFactory, IMessageSerializer messageSerializer, Handlers handlers, IPositionManager positionManager)
        {
            if (handlers == null) throw new ArgumentNullException(nameof(handlers));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _messageSerializer = messageSerializer ?? throw new ArgumentNullException(nameof(messageSerializer));
            _positionManager = positionManager ?? throw new ArgumentNullException(nameof(positionManager));
            _handlers = handlers.ToArray();
            _logger = loggerFactory.GetLogger(typeof(DefaultConsumerDispatcher));
        }

        public void Initialize()
        {
            foreach (var handler in _handlers)
            {
                var handlerType = handler.GetType();
                var logger = _loggerFactory.GetLogger(handlerType);

                handler.Start(logger);
            }

            _flusherLoopTask = Task.Run(async () => await RunPositionsFlusher());
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
            var positions = _handlers
                .SelectMany(h => h.GetPositions())
                .GroupBy(p => new { p.Topic, p.Partition })
                .Select(p => new Position(p.Key.Topic, p.Key.Partition, p.Min(a => a.Offset)))
                .ToList();

            if (!positions.Any()) return;

            _logger.Debug("Setting positions {@positions}", positions);

            await Task.WhenAll(positions.Select(position => _positionManager.Set(position)));
        }

        public void Dispatch(ReceivedTransportMessage transportMessage)
        {
            try
            {
                var receivedLogicalMessage = _messageSerializer.Deserialize(transportMessage);

                for (var index = 0; index < _handlers.Length; index++)
                {
                    var handler = _handlers[index];

                    while (!handler.IsReadyForMore)
                    {
                        Thread.Sleep(100);
                    }

                    handler.Enqueue(receivedLogicalMessage);
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Error in dispatcher");
            }
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