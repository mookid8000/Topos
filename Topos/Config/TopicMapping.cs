using System;

namespace Topos.Config
{
    public class TopicMapping
    {
        public Type Type { get; }
        public string Topic { get; }

        public TopicMapping(Type type, string topic)
        {
            Type = type;
            Topic = topic;
        }

        protected bool Equals(TopicMapping other)
        {
            return Type == other.Type && string.Equals(Topic, other.Topic);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TopicMapping) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Type != null ? Type.GetHashCode() : 0) * 397) ^ (Topic != null ? Topic.GetHashCode() : 0);
            }
        }

        public static bool operator ==(TopicMapping left, TopicMapping right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TopicMapping left, TopicMapping right)
        {
            return !Equals(left, right);
        }

        public override string ToString() => $"{Type} => {Topic}";
    }
}