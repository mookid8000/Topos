using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FASTER.core;
using Topos.Consumer;
using Topos.Extensions;
using Topos.Logging;
using Topos.Serialization;

namespace Topos.Faster
{
    class FasterLogConsumerImplementation : IConsumerImplementation, IDisposable
    {
        readonly CancellationTokenSource _cancellationTokenSource = new();
        readonly ILogEntrySerializer _logEntrySerializer;
        readonly IConsumerDispatcher _consumerDispatcher;
        readonly IPositionManager _positionManager;
        readonly IDeviceManager _deviceManager;
        readonly List<string> _topics;
        readonly ILogger _logger;

        IReadOnlyList<Task> _workers;

        public FasterLogConsumerImplementation(ILoggerFactory loggerFactory, IDeviceManager deviceManager, ILogEntrySerializer logEntrySerializer, IEnumerable<string> topics, IConsumerDispatcher consumerDispatcher, IPositionManager positionManager)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            if (topics == null) throw new ArgumentNullException(nameof(topics));
            _deviceManager = deviceManager ?? throw new ArgumentNullException(nameof(deviceManager));
            _logEntrySerializer = logEntrySerializer ?? throw new ArgumentNullException(nameof(logEntrySerializer));
            _consumerDispatcher = consumerDispatcher ?? throw new ArgumentNullException(nameof(consumerDispatcher));
            _positionManager = positionManager ?? throw new ArgumentNullException(nameof(positionManager));
            _logger = loggerFactory.GetLogger(GetType());
            _topics = topics.ToList();
        }

        public void Start()
        {
            if (_workers != null) throw new InvalidOperationException("Cannot start FasterLog consumer more than once");

            _workers = _topics
                .Select(topic => Task.Run(() => RunWorker(topic)))
                .ToList();
        }

        async Task RunWorker(string topic)
        {
            var cancellationToken = _cancellationTokenSource.Token;

            try
            {
                var log = _deviceManager.GetLog(topic);

                _logger.Debug("Starting FasterLog consumer task for topic {topic}", topic);

                var resumePosition = await GetResumePosition(topic, cancellationToken);
                var readAddress = GetReadAddress(resumePosition, log);

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        using var iterator = log.Scan(readAddress, long.MaxValue);

                        while (!cancellationToken.IsCancellationRequested)
                        {
                            while (iterator.GetNext(out var bytes, out _, out _, out var nextAddress))
                            {
                                var transportMessage = _logEntrySerializer.Deserialize(bytes);
                                var receivedTransportMessage = new ReceivedTransportMessage(
                                    new Position(topic, 0, nextAddress), transportMessage.Headers,
                                    transportMessage.Body);

                                _consumerDispatcher.Dispatch(receivedTransportMessage);

                                readAddress = nextAddress;
                            }

                            await iterator.WaitAsync(cancellationToken);
                        }
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        // we're on out way out
                        throw;
                    }
                    catch (Exception exception)
                    {
                        _logger.Error(exception, "Unhandled exception in FasterLog consumer task for topic {topic}", topic);
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // we're on out way out
            }

            _logger.Debug("Stopped FasterLog consumer task for topic {topic}", topic);
        }

        static long GetReadAddress(Position resumePosition, FasterLog log)
        {
            if (resumePosition.IsDefault) return log.BeginAddress;

            if (resumePosition.IsOnlyNew) return log.TailAddress;

            return resumePosition.Offset;
        }

        async Task<Position> GetResumePosition(string topic, CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var position = await _positionManager.Get(topic, 0);

                    return position ?? Position.Default(topic, 0);
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, "Error when trying to get resumt position for topic {topic} - waiting a while before trying again", topic);
                }

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
        }

        public void Dispose()
        {
            try
            {
                // not initialized or already disposed? who cares
                if (_workers == null) return;

                _logger.Info("Disposing FasterLog consumer");

                _cancellationTokenSource.Cancel();

                var timeout = TimeSpan.FromSeconds(4);

                if (!Task.WhenAll(_workers.ToArray()).WaitSafe(timeout))
                {
                    _logger.Warn("One or more background consumer tasks did not exit within timeout of {timeout}", timeout);
                }
            }
            finally
            {
                _workers = null;
                _cancellationTokenSource?.Dispose();
            }

        }
    }
}