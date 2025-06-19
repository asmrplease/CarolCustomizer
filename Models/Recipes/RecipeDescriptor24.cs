using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CarolCustomizer.Models.Recipes;
public class RecipeDescriptor24
{
    public string AnimatorSource;
    public string BaseOutfitName;
    public string ColliderSource;
    public IEnumerable<AccessoryDescriptor> ActiveAccessories;
    public IEnumerable<string> ActiveEffects;
    public string Hairstyle;
    public string HairMaterial;
    public string Version;

    [JsonConstructor]
    public RecipeDescriptor24(
        string animatorSource, 
        string baseOutfitName, 
        string colliderSource,
        IEnumerable<AccessoryDescriptor> activeAccessories,
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

    public RecipeDescriptor24(OutfitManager manager)
    {
        BaseOutfitName = manager.ConfigurationSource;
        AnimatorSource = manager.AnimatorSource;
        ColliderSource = manager.ColliderSource;
        ActiveAccessories = manager.LiveAccessoryDescriptors;
        ActiveEffects = manager.ActiveEffects;
        Hairstyle = manager.Hairstyle;
        HairMaterial = manager.HairColor;
        Version = PluginInfo.PLUGIN_VERSION;
    }
}
