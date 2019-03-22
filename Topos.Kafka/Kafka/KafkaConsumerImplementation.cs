using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Topos.Consumer;
using Topos.Logging;
using Topos.Serialization;
using static Topos.Internals.Callbacks;
// ReSharper disable RedundantAnonymousTypePropertyName
// ReSharper disable ArgumentsStyleNamedExpression

namespace Topos.Kafka
{
    public class KafkaConsumerImplementation : IConsumerImplementation, IDisposable
    {
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        readonly Action<ReceivedTransportMessage, CancellationToken> _eventHandler;
        readonly ManualResetEvent _consumerLoopExited = new ManualResetEvent(false);
        readonly IPositionManager _positionManager;
        readonly Thread _worker;
        readonly ILogger _logger;
        readonly string _address;
        readonly string[] _topics;
        readonly string _group;

        bool _disposed;

        public KafkaConsumerImplementation(ILoggerFactory loggerFactory, string address, IEnumerable<string> topics, string group,
            Action<ReceivedTransportMessage, CancellationToken> eventHandler, IPositionManager positionManager)
        {
            if (topics == null) throw new ArgumentNullException(nameof(topics));
            _logger = loggerFactory.GetLogger(typeof(KafkaConsumerImplementation));
            _address = address ?? throw new ArgumentNullException(nameof(address));
            _topics = topics.ToArray();
            _group = group ?? throw new ArgumentNullException(nameof(group));
            _eventHandler = eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));
            _positionManager = positionManager;
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
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _address,
                GroupId = _group,

                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
            };

            var consumer = new ConsumerBuilder<string, byte[]>(consumerConfig)
                .SetLogHandler((cns, message) => LogHandler(_logger, cns, message))
                .SetErrorHandler((cns, error) => ErrorHandler(_logger, cns, error))
                .SetPartitionsAssignedHandler((cns, partitions) => PartitionsAssigned(_logger, cns, partitions, _positionManager))
                .SetPartitionsRevokedHandler((cns, partitions) => PartitionsRevoked(_logger, cns, partitions))
                //.SetOffsetsCommittedHandler((cns, committedOffsets) => OffsetsCommitted(_logger, cns, committedOffsets))
                .Build();

            var topicsToSubscribeTo = new HashSet<string>(_topics);

            _logger.Info("Kafka consumer for group {consumerGroup} subscribing to topics: {topics}", _group, topicsToSubscribeTo);

            foreach (var topic in topicsToSubscribeTo)
            {
                consumer.Subscribe(topic);
            }

            var cancellationToken = _cancellationTokenSource.Token;

            _logger.Info("Starting Kafka consumer worker for group {consumerGroup}", _group);

            try
            {
                using (consumer)
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume(TimeSpan.FromSeconds(0.5));
                            if (consumeResult == null)
                            {
                                Thread.Sleep(23);
                                continue;
                            }

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
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Unhandled exception in Kafka consumer");
            }
            finally
            {
                _consumerLoopExited.Set();

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

                using (_cancellationTokenSource)
                {
                    if (_worker.ThreadState != ThreadState.Running) return;

                    if (!_worker.Join(TimeSpan.FromSeconds(5)))
                    {
                        _logger.Error("Kafka consumer worker for group {consumerGroup} did not finish executing within 5 s", _group);

                        _worker.Abort();
                    }

                    if (!_consumerLoopExited.WaitOne(TimeSpan.FromSeconds(2)))
                    {
                        _logger.Warn("Consumer loop for group {consumerGroup} did not exit within 2 s timeout", _group);
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