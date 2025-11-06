using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CarolCustomizer.Models.Recipes;
internal class RecipeDescriptor21
{
    internal string AnimatorSource;
    internal string BaseOutfitName;
    internal IEnumerable<LegacyAccDescriptor> ActiveAccessories;
    internal string Version;

    [JsonConstructor]
    internal RecipeDescriptor21(
        string animatorSource, 
        string baseOutfitName, 
        IEnumerable<LegacyAccDescriptor> activeAccessories, 
        string version)
    {
        AnimatorSource = animatorSource ?? Constants.Pyjamas;
        BaseOutfitName = baseOutfitName ?? Constants.Pyjamas;
        ActiveAccessories = activeAccessories;
        Version = version;
    }

    //public RecipeDescriptor21(OutfitCoordinator manager)
    //{
    //    BaseOutfitName = manager.ConfigurationSource;
    //    AnimatorSource = manager.AnimatorSource;
    //    ActiveAccessories = manager.LiveAccessoryDescriptors;
    //    Version = PluginInfo.PLUGIN_VERSION;
    //}
}
