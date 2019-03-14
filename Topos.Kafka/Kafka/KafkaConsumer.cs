using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Topos.Internals;
using Topos.Logging;
using Topos.EventProcessing;
using Topos.Serialization;

// ReSharper disable RedundantAnonymousTypePropertyName
// ReSharper disable ArgumentsStyleNamedExpression

namespace Topos.Kafka
{
    public class KafkaConsumer : IConsumerImplementation, IDisposable
    {
        static readonly Func<IEnumerable<Part>, Task> Noop = _ => Task.CompletedTask;

        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        readonly Action<ReceivedTransportMessage, CancellationToken> _eventHandler;
        readonly IConsumer<string, byte[]> _consumer;
        readonly Thread _worker;
        readonly ILogger _logger;
        readonly string _group;

        bool _disposed;

        public KafkaConsumer(ILoggerFactory loggerFactory, string address, IEnumerable<string> topics, string group,
            Action<ReceivedTransportMessage, CancellationToken> eventHandler,
            Func<IEnumerable<Part>, Task> partitionsAssigned = null,
            Func<IEnumerable<Part>, Task> partitionsRevoked = null)
        {
            _logger = loggerFactory.GetLogger(typeof(KafkaConsumer));
            _group = group ?? throw new ArgumentNullException(nameof(group));
            _eventHandler = eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = address,
                GroupId = group,

                AutoOffsetReset = AutoOffsetReset.Earliest,
                AutoCommitIntervalMs = 2000,
                EnableAutoCommit = true
            };

            _consumer = new ConsumerBuilder<string, byte[]>(consumerConfig)
                .SetLogHandler((consumer, message) => Handlers.LogHandler(_logger, consumer, message))
                .SetErrorHandler((consumer, error) => Handlers.ErrorHandler(_logger, consumer, error))
                .SetRebalanceHandler((consumer, rebalanceEvent) => Handlers.RebalanceHandler(
                    logger: _logger,
                    consumer: consumer,
                    rebalanceEvent: rebalanceEvent,
                    partitionsAssigned ?? Noop,
                    partitionsRevoked ?? Noop
                ))
                .SetOffsetsCommittedHandler((consumer, committedOffsets) =>
                    Handlers.OffsetsCommitted(_logger, consumer, committedOffsets))
                .Build();

            var topicsToSubscribeTo = new HashSet<string>(topics);

            _logger.Info("Kafka consumer for group {consumerGroup} subscribing to topics: {@topics}", _group, topicsToSubscribeTo);

            foreach (var topic in topicsToSubscribeTo)
            {
                _consumer.Subscribe(topic);
            }

            _worker = new Thread(Run) { IsBackground = true };
        }

        public void Start()
        {
            if (_worker.ThreadState == ThreadState.Running)
            {
                throw new InvalidOperationException("Kafka consumer worker is already running");
            }

            _worker.Start();
        }

        void Run()
        {
            var cancellationToken = _cancellationTokenSource.Token;

            _logger.Info("Starting Kafka consumer worker for group {consumerGroup}", _group);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(0.5));
                        if (consumeResult == null) continue;

                        var position = new Position(
                            topic: consumeResult.Topic,
                            partition: consumeResult.Partition.Value,
                            offset: consumeResult.Offset.Value
                        );

                        var message = new ReceivedTransportMessage(
                            position: position,
                            headers: GetHeaders(consumeResult.Headers),
                            body: consumeResult.Value
                        );

                        _logger.Debug("Received event {position}", position);

                        _eventHandler(message, cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        // it's alright
                    }
                    catch (ThreadAbortException)
                    {
                        _logger.Warn("Kafka consumer worker aborted!");
                        return;
                    }
                    catch (Exception exception)
                    {
                        _logger.Warn(exception, "Unhandled exception in Kafka consumer loop");

                        try
                        {
                            Task.Delay(TimeSpan.FromSeconds(30), cancellationToken)
                                .Wait(cancellationToken);
                        }
                        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                        {
                        }
                    }
                }
            }
            catch (AccessViolationException exception)
            {
                _logger.Error(exception, "CAN WE EVEN CATCH THIS?");
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Unhandled exception in Kafka consumer");
            }
            finally
            {
                _logger.Info("Kafka consumer worker for group {consumerGroup} stopped", _group);
            }
        }

        static Dictionary<string, string> GetHeaders(Headers headers)
        {
            var dictionary = new Dictionary<string, string>();

            foreach (var kvp in headers)
            {
                dictionary[kvp.Key] = Encoding.UTF8.GetString(kvp.GetValueBytes());
            }

            return dictionary;
        }

        public void Dispose()
        {
            if (_disposed) return;

            _logger.Info("Stopping Kafka consumer worker for group {consumerGroup}", _group);

            try
            {
                _cancellationTokenSource.Cancel();

                using (_consumer)
                using (_cancellationTokenSource)
                {
                    if (_worker.ThreadState != ThreadState.Running) return;

                    if (!_worker.Join(TimeSpan.FromSeconds(5)))
                    {
                        _logger.Error("Kafka consumer worker did not finish executing within 5 s");

                        _worker.Abort();
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