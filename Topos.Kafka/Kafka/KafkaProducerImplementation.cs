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
        static readonly object EmptyResult = new object();

        readonly IProducer<string, byte[]> _producer;
        readonly int _sendTimeoutSeconds;

        readonly ILogger _logger;

        bool _disposed;

        public KafkaProducerImplementation(ILoggerFactory loggerFactory, string address, int sendTimeoutSeconds = 30)
        {
            _logger = loggerFactory.GetLogger(typeof(KafkaProducerImplementation));
            _sendTimeoutSeconds = sendTimeoutSeconds;
            var config = new ProducerConfig { BootstrapServers = address };

            _producer = new ProducerBuilder<string, byte[]>(config)
                .SetLogHandler((producer, message) => LogHandler(_logger, producer, message))
                .SetErrorHandler((producer, message) => ErrorHandler(_logger, producer, message))
                .Build();

            _logger.Info("Kafka producer initialized with {address}", address);
        }

        public AdminClient GetAdminClient() => new AdminClient(_producer.Handle);

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

        //public Task SendAsync(string topic, IEnumerable<KafkaEvent> events)
        //{
        //    var taskCompletionSource = new TaskCompletionSource<object>();

        //    foreach (var evt in events)
        //    {
        //        var message = new Message<string, string>
        //        {
        //            Key = evt.Key,
        //            Headers = GetHeaders(evt.Headers),
        //            Value = evt.Body
        //        };
        //        _producer.BeginProduce(topic, message);
        //    }

        //    // is disposed in the finally block on the thread pool
        //    var cancellationTokenSource = new CancellationTokenSource();

        //    cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(_sendTimeoutSeconds));

        //    Task.Run(() =>
        //    {
        //        var cancellationToken = cancellationTokenSource.Token;

        //        try
        //        {
        //            _producer.Flush(cancellationToken);

        //            taskCompletionSource.SetResult(EmptyResult);
        //        }
        //        catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested)
        //        {
        //            taskCompletionSource.SetException(
        //                new TimeoutException($"Could not send events within {_sendTimeoutSeconds} s timeout",
        //                    exception));
        //        }
        //        catch (Exception exception)
        //        {
        //            taskCompletionSource.SetException(exception);
        //        }
        //        finally
        //        {
        //            cancellationTokenSource.Dispose();
        //        }
        //    });

        //    return taskCompletionSource.Task;
        //}

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
