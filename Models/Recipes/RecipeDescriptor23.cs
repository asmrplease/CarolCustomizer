using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CarolCustomizer.Models.Recipes;
public class RecipeDescriptor23
{
    public string AnimatorSource;
    public string BaseOutfitName;
    public string ColliderSource;
    public IEnumerable<AccessoryDescriptor> ActiveAccessories;
    public IEnumerable<string> ActiveEffects;
    public string Version;

    [JsonConstructor]
    public RecipeDescriptor23(
        string animatorSource, 
        string baseOutfitName, 
        string colliderSource,
        IEnumerable<AccessoryDescriptor> activeAccessories,
        IEnumerable<string> activeEffects,
        string version)
    {
        AnimatorSource = animatorSource ?? Constants.Pyjamas;
        BaseOutfitName = baseOutfitName ?? Constants.Pyjamas;
        ColliderSource = colliderSource ?? Constants.Pyjamas;
        ActiveAccessories = activeAccessories;
        ActiveEffects = activeEffects;
        Version = version;
    }

    public RecipeDescriptor23(OutfitManager manager)
    {
        BaseOutfitName = manager.ConfigurationSource;
        AnimatorSource = manager.AnimatorSource;
        ColliderSource = manager.ColliderSource;
        ActiveAccessories = manager.LiveAccessoryDescriptors;
        ActiveEffects = manager.ActiveEffects;
        Version = PluginInfo.PLUGIN_VERSION;
    }
}
