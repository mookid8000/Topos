using System;
using Topos.Routing;
using Topos.Serialization;

namespace Topos.Consumer
{
    public class DefaultToposConsumer : IToposConsumer
    {
        readonly IMessageSerializer _messageSerializer;
        readonly ITopicMapper _topicMapper;

        bool _disposed;

        public event Action Disposing;

        public DefaultToposConsumer(IMessageSerializer messageSerializer, ITopicMapper topicMapper)
        {
            _messageSerializer = messageSerializer;
            _topicMapper = topicMapper;
        }

        public void Start()
        {
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