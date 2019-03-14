using System;

namespace Topos.Producer
{
    public class TopicMapper
    {
        readonly TopicMappings _topicMappings;

        public TopicMapper(TopicMappings topicMappings)
        {
            _topicMappings = topicMappings;
        }

        public TopicMapper Map<T>(string topic)
        {
            _topicMappings.Add(typeof(T), topic);
            return this;
        }

        public TopicMapper Map(Type type, string topic)
        {
            _topicMappings.Add(type, topic);
            return this;
        }
    }
}