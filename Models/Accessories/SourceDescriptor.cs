using Newtonsoft.Json;
using System;

namespace CarolCustomizer.Models.Accessories;

public class SourceDescriptor : IEquatable<SourceDescriptor>
{
    public readonly string Name;
    public readonly SourceType Type;

    [JsonConstructor]
    public SourceDescriptor(string name, SourceType type)
    {
        Name = name; 
        Type = type;
    }

    public static implicit operator SourceDescriptor(string name) => new(name, SourceType.Outfit);

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

    public override string ToString()
    {
        return $"SD:{Type}-{Name}";
    }
}
