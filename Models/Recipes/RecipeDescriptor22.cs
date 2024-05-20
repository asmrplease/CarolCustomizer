using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CarolCustomizer.Models.Recipes;
public class RecipeDescriptor22
{
    public string AnimatorSource;
    public string BaseOutfitName;
    public IEnumerable<AccessoryDescriptor> ActiveAccessories;
    public IEnumerable<string> ActiveEffects;
    public string Version;

    [JsonConstructor]
    public RecipeDescriptor22(
        string animatorSource, 
        string baseOutfitName, 
        IEnumerable<AccessoryDescriptor> activeAccessories,
        IEnumerable<string> activeEffects,
        string version)
    {
        AnimatorSource = animatorSource ?? Constants.Pyjamas;
        BaseOutfitName = baseOutfitName ?? Constants.Pyjamas;
        ActiveAccessories = activeAccessories;
        ActiveEffects = activeEffects;
        Version = version;
    }

    public RecipeDescriptor22(OutfitManager manager)
    {
        BaseOutfitName = manager.ConfigurationSource;
        AnimatorSource = manager.AnimatorSource;
        ActiveAccessories = manager.LiveAccessoryDescriptors;
        ActiveEffects = manager.ActiveEffects;
        Version = PluginInfo.PLUGIN_VERSION;
    }
}
