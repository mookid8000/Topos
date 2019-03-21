using System;

namespace Topos.Consumer
{
    public struct Position
    {
        const int DefaultOffset = -1;

        public static Position Default(string topic, int partition) => new Position(topic, partition, DefaultOffset);

        public string Topic { get; }
        public int Partition { get; }
        public long Offset { get; }
        public bool IsDefault => Offset == DefaultOffset;

        public Position(string topic, int partition, long offset)
        {
            Topic = topic ?? throw new ArgumentNullException(nameof(topic));
            Partition = partition;
            Offset = offset;
        }

        public override string ToString() => $"{Topic}: {Partition}/{Offset}";

        public bool Equals(Position other)
        {
            return string.Equals(Topic, other.Topic) && Partition == other.Partition && Offset == other.Offset;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Position other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Topic.GetHashCode();
                hashCode = (hashCode * 397) ^ Partition;
                hashCode = (hashCode * 397) ^ Offset.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Position left, Position right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Position left, Position right)
        {
            return !left.Equals(right);
        }

        public Position Advance(int offset) => new Position(Topic, Partition, Offset + offset);
    }
}