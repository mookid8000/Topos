using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Topos.Internals;
using Topos.Logging;
using Topos.Serialization;
using static Topos.Internals.Callbacks;
// ReSharper disable MethodSupportsCancellation

namespace Topos.Kafka
{
    public class KafkaProducerImplementation : IProducerImplementation
    {
        static readonly Headers EmptyHeaders = new Headers();

        readonly IProducer<string, byte[]> _producer;
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

            _producer = new ProducerBuilder<string, byte[]>(config)
                .SetLogHandler((producer, message) => LogHandler(_logger, producer, message))
                .SetErrorHandler((producer, message) => ErrorHandler(_logger, producer, message))
                .Build();

            _logger.Info("Kafka producer initialized with {address}", config.BootstrapServers);

            _config = config;
        }

        public IAdminClient GetAdminClient()
        {
            var configuration = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("bootstrap.servers", _config.BootstrapServers),
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
            if (partitionKey == null) throw new ArgumentNullException(nameof(partitionKey));
            if (transportMessage == null) throw new ArgumentNullException(nameof(transportMessage));

            var headers = GetHeaders(transportMessage.Headers);
            var body = transportMessage.Body;

            var kafkaMessage = new Message<string, byte[]>
            {
                Key = partitionKey,
                Headers = headers,
                Value = body
            };

            await _producer.ProduceAsync(topic, kafkaMessage);
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
}
