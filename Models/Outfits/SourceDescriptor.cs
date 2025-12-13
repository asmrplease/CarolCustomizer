using Newtonsoft.Json;
using System;

namespace CarolCustomizer.Models.Outfits;

public record SourceDescriptor 
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
}
