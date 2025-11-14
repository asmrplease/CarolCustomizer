using JetBrains.Annotations;
using Newtonsoft.Json;
using System;

namespace CarolCustomizer.Models.Outfits;

public record SourceDescriptor : IEquatable<SourceDescriptor>, IComparable<SourceDescriptor>
{
    public string Name;
    public SourceType Type;

    [JsonConstructor]
    public SourceDescriptor(string name, SourceType type)
    {
        Name = name; 
        Type = type;
    }

    //This operator is implicit for the purposes of automatically converting old descriptors left in json
    public static implicit operator SourceDescriptor(string name) => new(name, SourceType.Outfit);

    public override string ToString()
    {
        return $"SD:{Type}-{Name}";
    }

    public int CompareTo(SourceDescriptor other)
    {
        //TODO: this comparison doesn't use the DisplayName of the outfits. 
        string thisName = LocalizationIndex.GetLine(this.Name);
        string thatName = LocalizationIndex.GetLine(other.Name);

        return thisName.CompareTo(thatName);
    }
}
