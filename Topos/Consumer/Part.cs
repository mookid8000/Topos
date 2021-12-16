using System;

namespace Topos.Consumer;

public struct Part
{
    public string Topic { get; }
    public int Partition { get; }

    public Part(string topic, int partition)
    {
        Topic = topic ?? throw new ArgumentNullException(nameof(topic));
        Partition = partition;
    }

    public override string ToString() => $"{Topic}: {Partition}";

    public bool Equals(Part other)
    {
        return string.Equals(Topic, other.Topic) && Partition == other.Partition;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Part other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Topic.GetHashCode() * 397) ^ Partition;
        }
    }

    public static bool operator ==(Part left, Part right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Part left, Part right)
    {
        return !left.Equals(right);
    }
}