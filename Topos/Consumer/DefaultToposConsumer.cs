using Topos.Routing;
using Topos.Serialization;

namespace Topos.Consumer
{
    public class DefaultToposConsumer : IToposConsumer
    {
        readonly IMessageSerializer _messageSerializer;
        readonly ITopicMapper _topicMapper;

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
            
        }
    }
}