using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Topos.Internals;
using Topos.Logging;
using Topos.Serialization;
using static Topos.Internals.Callbacks;
// ReSharper disable MethodSupportsCancellation

namespace Topos.Kafka;

public class KafkaProducerImplementation : IProducerImplementation
{
    static readonly Headers EmptyHeaders = new();

    readonly IProducer<string, byte[]> _producer;
    readonly int _kafkaOutgoingQueueMaxMessages;
    readonly ProducerConfig _config;
    readonly ILogger _logger;

    bool _disposed;

    public KafkaProducerImplementation(ILoggerFactory loggerFactory, string address, int sendTimeoutSeconds = 30, Func<ProducerConfig, ProducerConfig> configurationCustomizer = null)
    {
        _logger = loggerFactory.GetLogger(typeof(KafkaProducerImplementation));

        var config = new ProducerConfig
        {
            BootstrapServers = address,
            MessageTimeoutMs = sendTimeoutSeconds * 1000,
            LogConnectionClose = false,
        };

        if (configurationCustomizer != null)
        {
            config = configurationCustomizer(config);
        }

        _kafkaOutgoingQueueMaxMessages = config.QueueBufferingMaxMessages ?? 100000; 

        _producer = BuildProducer(config);

        _logger.Info("Kafka producer initialized with {address}", config.BootstrapServers);

        _config = config;
    }

    public IAdminClient GetAdminClient()
    {
        var configuration = new List<KeyValuePair<string, string>>
        {
            new("bootstrap.servers", _config.BootstrapServers),
        };

        if (!string.IsNullOrWhiteSpace(_config.SaslUsername))
        {
            configuration.Add(new KeyValuePair<string, string>("sasl.username", _config.SaslUsername));
            configuration.Add(new KeyValuePair<string, string>("sasl.password", _config.SaslPassword));
        }

        return new AdminClientBuilder(configuration).Build();
    }

    public async Task Send(string topic, string partitionKey, TransportMessage transportMessage)
    {
        if (topic == null) throw new ArgumentNullException(nameof(topic));
        if (transportMessage == null) throw new ArgumentNullException(nameof(transportMessage));

        var kafkaMessage = GetKafkaMessage(partitionKey, transportMessage);

        await _producer.ProduceAsync(topic, kafkaMessage);
    }

    public Task SendMany(string topic, string partitionKey, IEnumerable<TransportMessage> transportMessages)
    {
        if (topic == null) throw new ArgumentNullException(nameof(topic));
        if (transportMessages == null) throw new ArgumentNullException(nameof(transportMessages));

        var taskCompletionSource = new TaskCompletionSource<object>();

        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                foreach (var batch in transportMessages.Batch(_kafkaOutgoingQueueMaxMessages))
                {
                    foreach (var transportMessage in batch)
                    {
                        var kafkaMessage = GetKafkaMessage(partitionKey, transportMessage);
                        _producer.Produce(topic, kafkaMessage);
                    }

                    _producer.Flush();
                }

                Task.Run(() => taskCompletionSource.SetResult(null));
            }
            catch (Exception exception)
            {
                Task.Run(() => taskCompletionSource.SetException(exception));
            }
        });

        return taskCompletionSource.Task;
    }

    IProducer<string, byte[]> BuildProducer(Confluent.Kafka.Config config)
    {
        try
        {
            return new ProducerBuilder<string, byte[]>(config)
                .SetLogHandler((producer, message) => LogHandler(_logger, producer, message))
                .SetErrorHandler((producer, message) => ErrorHandler(_logger, producer, message))
                .Build();
        }
        catch (Exception exception)
        {
            throw new ArgumentException($@"Could not build Kafka producer with the following properties:

{string.Join(Environment.NewLine, config.Select(kvp => $"    {kvp.Key}={kvp.Value}"))}", exception);
        }
    }

    static Headers GetHeaders(Dictionary<string, string> dictionary)
    {
        if (dictionary.Count == 0) return EmptyHeaders;

        var headers = new Headers();

        foreach (var (key, value) in dictionary)
        {
            headers.Add(key, Encoding.UTF8.GetBytes(value));
        }

        return headers;
    }

    static Message<string, byte[]> GetKafkaMessage(string partitionKey, TransportMessage transportMessage)
    {
        var headers = GetHeaders(transportMessage.Headers);
        var body = transportMessage.Body;

        var kafkaMessage = new Message<string, byte[]>
        {
            Key = partitionKey,
            Headers = headers,
            Value = body
        };

        return kafkaMessage;
    }

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            _logger.Info("Disposing Kafka producer");

            _producer.Dispose();
        }
        finally
        {
            _disposed = true;
        }
    }
}