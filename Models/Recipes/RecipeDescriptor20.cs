using CarolCustomizer.Models.Accessories;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CarolCustomizer.Models.Recipes;
internal class RecipeDescriptor20
{
    internal string BaseOutfitName;
    internal bool BaseVisible;
    internal IEnumerable<LegacyAccDescriptor> ActiveAccessories;
    internal string Version;

    [JsonConstructor]
    internal RecipeDescriptor20(
        string baseOutfitName, 
        bool baseVisible, 
        IEnumerable<LegacyAccDescriptor> activeAccessories, 
        string version)
    {
        BaseOutfitName = baseOutfitName;
        BaseVisible = baseVisible;
        ActiveAccessories = activeAccessories;
        Version = version;
    }
}