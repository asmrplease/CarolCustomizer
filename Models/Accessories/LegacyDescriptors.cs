using CarolCustomizer.Models.Materials;

namespace CarolCustomizer.Models.Accessories;
internal class LegacyMatDescriptor
{
    public string Name;
    public string Source;
    public SourceType SourceType;

    public static implicit operator MaterialDescriptor(LegacyMatDescriptor descriptor)
    {
        return new MaterialDescriptor(descriptor.Name, new SourceDescriptor(descriptor.Source, descriptor.SourceType));
    }
}

internal class LegacyAccDescriptor
{
    public string Name;
    public string Source;
    public MaterialDescriptor[] Materials;

    public static implicit operator AccessoryDescriptor(LegacyAccDescriptor descriptor)
    {
        return new AccessoryDescriptor(descriptor.Name, descriptor.Source, descriptor.Materials);
    }
}

