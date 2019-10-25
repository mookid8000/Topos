using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Topos.Extensions;
using Topos.Logging;
using Topos.Routing;
using Topos.Serialization;

namespace Topos.Producer
{
    public class DefaultToposProducer : IToposProducer
    {
        readonly IMessageSerializer _messageSerializer;
        readonly ITopicMapper _topicMapper;
        readonly IProducerImplementation _producerImplementation;
        readonly ILogger _logger;

        bool _disposing;
        bool _disposed;

        public event Action Disposing;

        public DefaultToposProducer(IMessageSerializer messageSerializer, ITopicMapper topicMapper, IProducerImplementation producerImplementation, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.GetLogger(typeof(DefaultToposProducer));
            _messageSerializer = messageSerializer ?? throw new ArgumentNullException(nameof(messageSerializer));
            _topicMapper = topicMapper ?? throw new ArgumentNullException(nameof(topicMapper));
            _producerImplementation = producerImplementation ?? throw new ArgumentNullException(nameof(producerImplementation));
        }

        public async Task Send(ToposMessage message, string partitionKey = null)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var body = message.Body;
            var topic = _topicMapper.GetTopic(body);

            var headersOrNull = message.Headers;
            var headers = headersOrNull?.Clone() ?? new Dictionary<string, string>();

            if (!headers.ContainsKey(ToposHeaders.MessageId))
            {
                headers[ToposHeaders.MessageId] = Guid.NewGuid().ToString();
            }

            var logicalMessage = new LogicalMessage(headers, body);
            var transportMessage = _messageSerializer.Serialize(logicalMessage);

            _logger.Debug("Sending message with ID {messageId} to topic {topic}", logicalMessage.GetMessageId(), topic);

            await _producerImplementation.Send(topic, partitionKey, transportMessage);
        }

        public async Task SendMany(IEnumerable<ToposMessage> messages, string partitionKey = null)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));
        }

        /// <summary>
        /// Guard agains double-entry, because the callback will end up disposing this instance too
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            if (_disposing) return;

            _disposing = true;

            try
            {
                Disposing?.Invoke();
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}