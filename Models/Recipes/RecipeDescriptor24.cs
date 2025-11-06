using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CarolCustomizer.Models.Recipes;
internal class RecipeDescriptor24
{
    internal string AnimatorSource;
    internal string BaseOutfitName;
    internal string ColliderSource;
    internal IEnumerable<LegacyAccDescriptor> ActiveAccessories;
    internal IEnumerable<string> ActiveEffects;
    internal string Hairstyle;
    internal string HairMaterial;
    internal string Version;

    [JsonConstructor]
    internal RecipeDescriptor24(
        string animatorSource, 
        string baseOutfitName, 
        string colliderSource,
        IEnumerable<LegacyAccDescriptor> activeAccessories,
        IEnumerable<string> activeEffects,
        string hairstyle,
        string hairMaterial,
        string version)
    {
        AnimatorSource = animatorSource ?? Constants.Pyjamas;
        BaseOutfitName = baseOutfitName ?? Constants.Pyjamas;
        ColliderSource = colliderSource ?? Constants.Pyjamas;
        ActiveAccessories = activeAccessories;
        ActiveEffects = activeEffects;
        Hairstyle = hairstyle;
        HairMaterial = hairMaterial;
        Version = version;
    }

    //public RecipeDescriptor24(OutfitCoordinator manager)
    //{
    //    BaseOutfitName = manager.ConfigurationSource;
    //    AnimatorSource = manager.AnimatorSource;
    //    ColliderSource = manager.ColliderSource;
    //    ActiveAccessories = manager.LiveAccessoryDescriptors;
    //    ActiveEffects = manager.ActiveEffects;
    //    Hairstyle = manager.HairstyleName;
    //    HairMaterial = manager.HairMaterialName;
    //    Version = PluginInfo.PLUGIN_VERSION;
    //}
}
