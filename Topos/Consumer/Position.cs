using System;

namespace Topos.Consumer;

public class Position
{
    public const int DefaultOffset = -1;

    public const int OnlyNewOffset = -2;

    /// <summary>
    /// Gets the "default" position, which is the lowest possible position. Brokers should interpret this position as "as much as you've got", so
    /// passing this position when resuming should get all events available in the broker
    /// </summary>
    public static Position Default(string topic, int partition) => new Position(topic, partition, DefaultOffset);

    /// <summary>
    /// Gets the "only new" position, which brokers should interpret as "only new events", so
    /// passing this position when resuming should get only events written after the consumer starts
    /// </summary>
    public static Position OnlyNew(string topic, int partition) => new Position(topic, partition, OnlyNewOffset);

    public string Topic { get; }
    public int Partition { get; }
    public long Offset { get; }

    public bool IsDefault => Offset == DefaultOffset;
        
    public bool IsOnlyNew => Offset == OnlyNewOffset;

    public Position(string topic, int partition, long offset)
    {
        Topic = topic ?? throw new ArgumentNullException(nameof(topic));
        Partition = partition;
        Offset = offset;
    }

    public override string ToString() => $"{Topic}: {Partition}/{Offset}";

    public bool Equals(Position other) => string.Equals(Topic, other.Topic) && Partition == other.Partition && Offset == other.Offset;

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

    public static bool operator ==(Position left, Position right) => left.Equals(right);

    public static bool operator !=(Position left, Position right) => !left.Equals(right);

    public Position Advance(int offset) => new Position(Topic, Partition, Offset + offset);
}