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
// ReSharper disable ArgumentsStyleOther
// ReSharper disable EmptyGeneralCatchClause

namespace Topos.Kafka
{
    public class KafkaConsumerImplementation : IConsumerImplementation, IDisposable
    {
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        readonly Func<ConsumerContext, IEnumerable<TopicPartition>, Task> _partitionsAssignedHandler;
        readonly Func<ConsumerContext, IEnumerable<TopicPartition>, Task> _partitionsRevokedHandler;
        readonly Func<ConsumerConfig, ConsumerConfig> _configurationCustomizer;
        readonly IConsumerDispatcher _consumerDispatcher;
        readonly IPositionManager _positionManager;
        readonly ConsumerContext _context;
        readonly Thread _worker;
        readonly ILogger _logger;
        readonly string _address;
        readonly string[] _topics;
        readonly string _group;

        bool _disposed;

        public KafkaConsumerImplementation(ILoggerFactory loggerFactory, string address, IEnumerable<string> topics, string group,
            IConsumerDispatcher consumerDispatcher, IPositionManager positionManager,
            ConsumerContext context,
            Func<ConsumerConfig, ConsumerConfig> configurationCustomizer = null,
            Func<ConsumerContext, IEnumerable<TopicPartition>, Task> partitionsAssignedHandler = null,
            Func<ConsumerContext, IEnumerable<TopicPartition>, Task> partitionsRevokedHandler = null)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            if (topics == null) throw new ArgumentNullException(nameof(topics));
            _logger = loggerFactory.GetLogger(typeof(KafkaConsumerImplementation));
            _address = address ?? throw new ArgumentNullException(nameof(address));
            _group = group ?? throw new ArgumentNullException(nameof(group));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _consumerDispatcher = consumerDispatcher ?? throw new ArgumentNullException(nameof(consumerDispatcher));
            _topics = topics.ToArray();
            _positionManager = positionManager;
            _configurationCustomizer = configurationCustomizer;
            _partitionsAssignedHandler = partitionsAssignedHandler;
            _partitionsRevokedHandler = partitionsRevokedHandler;
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

            if (_configurationCustomizer != null)
            {
                consumerConfig = _configurationCustomizer(consumerConfig);
            }

            var consumer = new ConsumerBuilder<string, byte[]>(consumerConfig)
                .SetLogHandler((cns, message) => LogHandler(_logger, cns, message))
                .SetErrorHandler((cns, error) => ErrorHandler(_logger, cns, error))
                .SetPartitionsAssignedHandler((cns, partitions) => PartitionsAssigned(_logger, partitions, _positionManager, _partitionsAssignedHandler, _context))
                .SetPartitionsRevokedHandler((cns, partitions) => PartitionsRevoked(_logger, partitions, _partitionsRevokedHandler, _context))
                .Build();

            var topicsToSubscribeTo = new HashSet<string>(_topics);

            _logger.Info("Kafka consumer for group {consumerGroup} subscribing to topics: {topics}", _group, topicsToSubscribeTo);

            consumer.Subscribe(topicsToSubscribeTo);

            var cancellationToken = _cancellationTokenSource.Token;

            _logger.Info("Starting Kafka consumer worker for group {consumerGroup}", _group);

            using (consumer)
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        TryProcessNextMessage(consumer, cancellationToken);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // it's alright
                }
                catch (ThreadAbortException)
                {
                    _logger.Warn("Kafka consumer worker aborted!");
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, "Unhandled exception in Kafka consumer");
                }
                finally
                {
                    try
                    {
                        consumer.Close();
                    }
                    catch
                    {
                    }

                    _logger.Info("Kafka consumer worker for group {consumerGroup} stopped", _group);
                }
            }
        }

        void TryProcessNextMessage(IConsumer<string, byte[]> consumer, CancellationToken cancellationToken)
        {
            try
            {
                var consumeResult = consumer.Consume(cancellationToken);
                if (consumeResult == null)
                {
                    Thread.Sleep(100); //< chill (but it should not happen)
                    return;
                }

                var position = new Position(
                    topic: consumeResult.Topic,
                    partition: consumeResult.Partition.Value,
                    offset: consumeResult.Offset.Value
                );

                var kafkaMessage = consumeResult.Message;
                var headers = kafkaMessage?.Headers;
                var body = kafkaMessage?.Value;

                var message = new ReceivedTransportMessage(
                    position: position,
                    headers: headers != null ? GetHeaders(headers) : new Dictionary<string, string>(),
                    body: body
                );

                _logger.Debug("Received event {position}", position);

                _consumerDispatcher.Dispatch(message);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // it's alright
            }
            catch (ThreadAbortException)
            {
                _logger.Warn("Kafka consumer worker aborted!");
            }
            catch (Exception exception)
            {
                _logger.Warn(exception, "Unhandled exception in Kafka consumer loop - waiting 30 s");

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
                }
            }
            finally
            {
                _disposed = true;

                _logger.Info("Kafka consumer for group {consumerGroup} stopped", _group);
            }
        }
    }
}