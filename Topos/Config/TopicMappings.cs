using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Topos.Routing;

namespace Topos.Config
{
    public class TopicMappings : IEnumerable<TopicMapping>
    {
        readonly HashSet<TopicMapping> _mappings = new HashSet<TopicMapping>();

        public IEnumerator<TopicMapping> GetEnumerator() => _mappings.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(Type type, string topic)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (topic == null) throw new ArgumentNullException(nameof(topic));
            
            _mappings.Add(new TopicMapping(type, topic));
        }

        public ITopicMapper BuildTopicMapper() => new TopicMapperImpl(_mappings);

        class TopicMapperImpl : ITopicMapper
        {
            readonly ConcurrentDictionary<Type, string> _map = new ConcurrentDictionary<Type, string>();

            public TopicMapperImpl(IEnumerable<TopicMapping> mappings)
            {
                foreach (var mapping in mappings)
                {
                    if (_map.TryAdd(mapping.Type, mapping.Topic)) continue;

                    throw new ArgumentException($"Could not add topic mapping {mapping}, because a mapping already exists for {mapping.Type}");
                }
            }

            public string GetTopic(object message)
            {
                if (message == null) throw new ArgumentNullException(nameof(message));
                var type = message.GetType();
                return _map.TryGetValue(type, out var topic)
                    ? topic
                    : throw new ArgumentException($"Could not find topic to send message of type {type} to");
            }
        }
    }
}