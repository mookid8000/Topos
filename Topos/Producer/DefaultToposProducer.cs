using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Topos.Routing;
using Topos.Serialization;

namespace Topos.Producer
{
    public class DefaultToposProducer : IToposProducer
    {
        readonly IMessageSerializer _messageSerializer;
        readonly ITopicMapper _topicMapper;

        bool _disposed;

        public event Action Disposing;

        public DefaultToposProducer(IMessageSerializer messageSerializer, ITopicMapper topicMapper)
        {
            _messageSerializer = messageSerializer ?? throw new ArgumentNullException(nameof(messageSerializer));
            _topicMapper = topicMapper ?? throw new ArgumentNullException(nameof(topicMapper));
        }

        public async Task Send(object message, Dictionary<string, string> heaoptionalHeadersders = null)
        {
            var topic = _topicMapper.GetTopic(message);
        }

        public void Dispose()
        {
            if (_disposed) return;

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