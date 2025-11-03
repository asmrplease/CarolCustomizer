using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarolCustomizer.Models.Accessories;

public class SourceDescriptor : IEquatable<SourceDescriptor>, IComparable<SourceDescriptor>
{
    public readonly SourceType Type;
    public readonly string Name;

    [JsonConstructor]
    public SourceDescriptor(string name, SourceType type = 0)
    {
        Name = name; 
        Type = type;
    }

    public bool Equals(SourceDescriptor other)
    {
        if (other is null) { return false; }
        if (ReferenceEquals(this, other)) { return true; }

        return Type == other.Type
            && Name == other.Name;
    }

    public override bool Equals(object other)
    {
        if (other is null) return false;
        if (other.GetType() != GetType()) return false;
        return base.Equals((SourceDescriptor) other);
    }

    public override int GetHashCode()
    {
        return Type.GetHashCode() ^ Name.GetHashCode();
    }

    public int CompareTo(SourceDescriptor other)
    {
        return Name.CompareTo(other.Name);
    }

    public override string ToString()
    {
        return $"SD:{Type}-{Name}";
    }
}
