using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Accessories;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace CarolCustomizer.Models.Recipes;
public class RecipeDescriptor20
{
    public string BaseOutfitName;
    public bool BaseVisible;
    public IEnumerable<AccessoryDescriptor> ActiveAccessories;
    public string Version;

    [JsonConstructor]
    public RecipeDescriptor20(string baseOutfitName, bool baseVisible, IEnumerable<AccessoryDescriptor> activeAccessories, string version)
    {
        BaseOutfitName = baseOutfitName;
        BaseVisible = baseVisible;
        ActiveAccessories = activeAccessories;
        Version = version;
    }
}