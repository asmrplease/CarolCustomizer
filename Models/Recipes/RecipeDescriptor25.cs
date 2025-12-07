using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace CarolCustomizer.Models.Recipes;
public class RecipeDescriptor25
{
    public SourceDescriptor AnimatorSource;
    public SourceDescriptor ConfigurationSource;
    public SourceDescriptor ColliderSource;
    public IEnumerable<AccessoryDescriptor> ActiveAccessories;
    public IEnumerable<SourceDescriptor> ActiveEffects;
    public string Version;

    [JsonConstructor]
    public RecipeDescriptor25(
        SourceDescriptor animatorSource, 
        SourceDescriptor baseOutfitName, 
        SourceDescriptor colliderSource,
        IEnumerable<AccessoryDescriptor> activeAccessories,
        IEnumerable<SourceDescriptor> activeEffects,
        string version)
    {
        AnimatorSource = animatorSource ?? Constants.PyjamaDescriptor;
        ConfigurationSource = baseOutfitName ?? Constants.PyjamaDescriptor;
        ColliderSource = colliderSource ?? Constants.PyjamaDescriptor;
        ActiveAccessories = activeAccessories;
        ActiveEffects = activeEffects;
        Version = version;
    }

    public RecipeDescriptor25(OutfitCoordinator manager)
    {
        ConfigurationSource = manager.ConfigurationSource;
        AnimatorSource = manager.AnimatorSource;
        ColliderSource = manager.ColliderSource;
        ActiveAccessories = manager.LiveAccessoryDescriptors.ToList();
        ActiveEffects = manager.ActiveEffects.ToList();
        Version = PluginInfo.PLUGIN_VERSION;
    }
}
