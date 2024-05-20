namespace CarolCustomizer.Models;
public record OutfitEffect
{
    public OutfitEffect(string RelativePath, ComponentType type)
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
