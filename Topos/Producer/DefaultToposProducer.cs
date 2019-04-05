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

        public async Task Send(object message, string partitionKey = null, Dictionary<string, string> optionalHeaders = null)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var topic = _topicMapper.GetTopic(message);
            var headers = optionalHeaders ?? new Dictionary<string, string>();

            if (!headers.ContainsKey(ToposHeaders.MessageId))
            {
                headers[ToposHeaders.MessageId] = Guid.NewGuid().ToString();
            }

            var logicalMessage = new LogicalMessage(headers, message);
            var transportMessage = _messageSerializer.Serialize(logicalMessage);

            _logger.Debug("Sending message with ID {messageId} to topic {topic}", logicalMessage.GetMessageId(), topic);

            await _producerImplementation.Send(topic, partitionKey ?? "", transportMessage);
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