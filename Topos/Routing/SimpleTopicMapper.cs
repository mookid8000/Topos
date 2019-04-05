using System;

namespace Topos.Routing
{
    /// <summary>
    /// Implementation of <see cref="ITopicMapper"/> that gets the class name of the event type and generates a topic out of that.
    /// E.g. sending events of type <see cref="DateTime"/> would yield a topic named "datetime"
    /// </summary>
    public class SimpleTopicMapper : ITopicMapper
    {
        public string GetTopic(object message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            return message.GetType().Name.ToLowerInvariant();
        }
    }
}