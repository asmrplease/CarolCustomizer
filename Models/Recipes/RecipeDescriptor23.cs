using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CarolCustomizer.Models.Recipes;
internal class RecipeDescriptor23
{
    internal string AnimatorSource;
    internal string BaseOutfitName;
    internal string ColliderSource;
    internal IEnumerable<LegacyAccDescriptor> ActiveAccessories;
    internal IEnumerable<string> ActiveEffects;
    internal string Version;

    [JsonConstructor]
    internal RecipeDescriptor23(
        string animatorSource, 
        string baseOutfitName, 
        string colliderSource,
        IEnumerable<LegacyAccDescriptor> activeAccessories,
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

    //internal RecipeDescriptor23(OutfitCoordinator manager)
    //{
    //    BaseOutfitName = manager.ConfigurationSource;
    //    AnimatorSource = manager.AnimatorSource;
    //    ColliderSource = manager.ColliderSource;
    //    ActiveAccessories = manager.LiveAccessoryDescriptors;
    //    ActiveEffects = manager.ActiveEffects;
    //    Version = PluginInfo.PLUGIN_VERSION;
    //}
}
