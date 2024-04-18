using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using CarolCustomizer.Behaviors;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Accessories;

namespace CarolCustomizer.Models.Recipes;
public class RecipeDescriptor20 : IRecipeDescriptor
{
    public string BaseOutfitName;
    public int BaseAccessorySlot;
    public bool BaseVisible;
    public IEnumerable<AccessoryDescriptor> ActiveAccessories;
    public string Version;

    [JsonConstructor]
    public RecipeDescriptor20(string baseOutfitName, int baseAccessorySlot, bool baseVisible, IEnumerable<AccessoryDescriptor> activeAccessories, string version)
    {
        BaseOutfitName = baseOutfitName;
        BaseAccessorySlot = baseAccessorySlot;
        BaseVisible = baseVisible;
        ActiveAccessories = activeAccessories;
        Version = version;
    }

    public RecipeDescriptor20(OutfitManager manager)
    {
        BaseOutfitName = manager.BaseOutfitName;
        BaseAccessorySlot = manager.BaseAccessorySlot;
        BaseVisible = manager.BaseVisible;
        ActiveAccessories = manager.ActiveAccessories
            .Select(x => new AccessoryDescriptor(x));
        Version = PluginInfo.PLUGIN_VERSION;
    }
}