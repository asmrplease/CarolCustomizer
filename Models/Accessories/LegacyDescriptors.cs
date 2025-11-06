using CarolCustomizer.Models.Materials;
using CarolCustomizer.Models.Outfits;
using Newtonsoft.Json;
using System.Linq;

namespace CarolCustomizer.Models.Accessories;
internal class LegacyMatDescriptor
{
    public string Name;
    public string Source;
    public SourceType SourceType;

    [JsonConstructor]
    internal LegacyMatDescriptor(string name, string source, SourceType sourceType)
    {
        Name = name;
        Source = source;
        SourceType = sourceType;
    }

    public static explicit operator MaterialDescriptor(LegacyMatDescriptor descriptor)
    {
        return new MaterialDescriptor
        (
            descriptor.Name, 
            new SourceDescriptor(descriptor.Source, descriptor.SourceType)
        );
    }
}

internal class LegacyAccDescriptor
{
    public string Name;
    public string Source;
    public LegacyMatDescriptor[] Materials;

    public static implicit operator AccessoryDescriptor(LegacyAccDescriptor descriptor)
    {
        return new AccessoryDescriptor
        (
            descriptor.Name,
            new SourceDescriptor(descriptor.Source, SourceType.Outfit),
            descriptor.Materials
                .Select(x => (MaterialDescriptor) x)
                .ToArray()
        );
    }
}

