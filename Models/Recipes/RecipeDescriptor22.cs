using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CarolCustomizer.Models.Recipes;
internal class RecipeDescriptor22
{
    internal string AnimatorSource;
    internal string BaseOutfitName;
    internal IEnumerable<LegacyAccDescriptor> ActiveAccessories;
    internal IEnumerable<string> ActiveEffects;
    internal string Version;

    [JsonConstructor]
    internal RecipeDescriptor22(
        string animatorSource, 
        string baseOutfitName, 
        IEnumerable<LegacyAccDescriptor> activeAccessories,
        IEnumerable<string> activeEffects,
        string version)
    {
        AnimatorSource = animatorSource ?? Constants.Pyjamas;
        BaseOutfitName = baseOutfitName ?? Constants.Pyjamas;
        ActiveAccessories = activeAccessories;
        ActiveEffects = activeEffects;
        Version = version;
    }

    //public RecipeDescriptor22(OutfitCoordinator manager)
    //{
    //    BaseOutfitName = manager.ConfigurationSource;
    //    AnimatorSource = manager.AnimatorSource;
    //    ActiveAccessories = manager.LiveAccessoryDescriptors;
    //    ActiveEffects = manager.ActiveEffects;
    //    Version = PluginInfo.PLUGIN_VERSION;
    //}
}
