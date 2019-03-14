using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Topos.Internals;
using Topos.Logging;
using Topos.EventProcessing;
// ReSharper disable RedundantAnonymousTypePropertyName
// ReSharper disable ArgumentsStyleNamedExpression

namespace Topos.Kafka
{
    public class KafkaConsumer : IToposConsumerImplementation
    {
        static readonly Func<IEnumerable<Part>, Task> Noop = _ => Task.CompletedTask;

        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        readonly Func<KafkaEvent, Position, CancellationToken, Task> _eventHandler;
        readonly IConsumer<string, string> _consumer;
        readonly Thread _worker;
        readonly ILogger _logger;
        readonly string _group;

        public KafkaConsumer(ILoggerFactory loggerFactory, string address, IEnumerable<string> topics, string group,
            Func<KafkaEvent, Position, CancellationToken, Task> eventHandler,
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

            _consumer = new ConsumerBuilder<string, string>(consumerConfig)
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
                        if (consumeResult == null)
                        {
                            continue;
                        }

                        var kafkaEvent = new KafkaEvent(
                            consumeResult.Key,
                            consumeResult.Value,
                            GetHeaders(consumeResult.Headers)
                        );

                        var topf = consumeResult.TopicPartitionOffset;

                        _logger.Debug("Received event: {@event} - {@position}", kafkaEvent, new { Topic = topf.Topic, Offset = $"{topf.Partition.Value}/{topf.Offset.Value}" });

                        var position = new Position(consumeResult.Topic, consumeResult.Partition.Value, consumeResult.Offset.Value);

                        _eventHandler(kafkaEvent, position, cancellationToken).Wait(cancellationToken);
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
    }
}