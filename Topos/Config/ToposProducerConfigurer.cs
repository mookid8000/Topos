using System;
using Topos.Internals;
using Topos.Routing;

namespace Topos.Config
{
    public class ToposProducerConfigurer
    {
        internal readonly Injectionist _injectionist = new Injectionist();

        readonly TopicMappings _topicMappings = new TopicMappings();

        public ToposProducerConfigurer(Action<StandardConfigurer<IProducerImplementation>> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            var configurer = StandardConfigurer<IProducerImplementation>.New(_injectionist);

            configure(configurer);
        }

        public ToposProducerConfigurer Topics(Action<TopicMapper> mapper)
        {
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));

            if (!_injectionist.Has<ITopicMapper>())
            {
                _injectionist.Register(c => _topicMappings.BuildTopicMapper());
            }

            mapper(new TopicMapper(_topicMappings));
            return this;
        }
    }
}