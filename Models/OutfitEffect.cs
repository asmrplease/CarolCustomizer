using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;

namespace CarolCustomizer.Models;
public class OutfitEffect : AccessoryDescriptor
{
    public OutfitEffect(string RelativePath, ComponentType type, SourceDescriptor source) : 
        base("__effects", source)
    {
        this.RelativePath = RelativePath;
        this.Type = type;
    }

    public readonly string RelativePath;
    public readonly ComponentType Type;
    public enum ComponentType
    {
         Behavior = 0
        ,Component = 1
    }
}
