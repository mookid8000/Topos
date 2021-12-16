using System;

namespace Topos.Config;

public class GroupId
{
    public string Id { get; }

    public GroupId(string id)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
    }

    protected bool Equals(GroupId other)
    {
        return string.Equals(Id, other.Id);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((GroupId) obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(GroupId left, GroupId right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(GroupId left, GroupId right)
    {
        return !Equals(left, right);
    }
}