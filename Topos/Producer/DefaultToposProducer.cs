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

        public DefaultToposProducer(IMessageSerializer messageSerializer, ITopicMapper topicMapper)
        {
            _messageSerializer = messageSerializer ?? throw new ArgumentNullException(nameof(messageSerializer));
            _topicMapper = topicMapper ?? throw new ArgumentNullException(nameof(topicMapper));
        }

        public async Task Send(object message, IDictionary<string, string> optionalHeaders = null)
        {
            var topic = _topicMapper.GetTopic(message);


        }

        public async Task SendMany(object message, IDictionary<string, string> optionalHeaders = null)
        {
            var topic = _topicMapper.GetTopic(message);


        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}