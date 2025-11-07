using CarolCustomizer.Models.Materials;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using Newtonsoft.Json;
using System.Linq;

namespace CarolCustomizer.Models.Accessories;
internal class LegacyMatDescriptor
{
    public string Name;
    public string Source;
    public SourceType Type;

    [JsonConstructor]
    internal LegacyMatDescriptor(string name, string source, SourceType sourceType)
    {
        //Log.Debug($"LegacyMatDescriptor.ctor({name},{source},{sourceType})");
        Name = name;
        Source = source;
        Type = sourceType;
    }

    public static explicit operator MaterialDescriptor(LegacyMatDescriptor legacy)
    {
        return new MaterialDescriptor
        (
            legacy.Name, 
            new SourceDescriptor(legacy.Source, legacy.Type)
        );
    }
}

internal class LegacyAccDescriptor
{
    public string Name;
    public string Source;
    public LegacyMatDescriptor[] Materials;

    [JsonConstructor]
    internal LegacyAccDescriptor(string name, string source, LegacyMatDescriptor[] materials)
    {
        //Log.Debug($"LegacyAccDescriptor.ctor({name}, {source}, {materials})");
        Name = name;
        Source = source;
        Materials = materials;
    }

    public static implicit operator AccessoryDescriptor(LegacyAccDescriptor legacy)
    {
        return new AccessoryDescriptor
        (
            legacy.Name,
            new SourceDescriptor(legacy.Source, SourceType.Outfit),
            legacy.Materials
                .Select(x => (MaterialDescriptor) x)
                .ToArray()
        );
    }
}

