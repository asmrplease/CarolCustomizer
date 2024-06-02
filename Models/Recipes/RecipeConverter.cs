using CarolCustomizer.Utils;
using System.Collections.Generic;

namespace CarolCustomizer.Models.Recipes;
public static class RecipeConverter
{
    public static RecipeDescriptor21 ToVersion210(this RecipeDescriptor20 legacy)
    {
        return new RecipeDescriptor21(
            legacy.BaseOutfitName,
            legacy.BaseOutfitName,
            legacy.ActiveAccessories,
            "2.1.0");
    }

    public static RecipeDescriptor22 ToVersion220(this RecipeDescriptor21 legacy) 
    {
        return new RecipeDescriptor22(
            legacy.AnimatorSource,
            legacy.BaseOutfitName,
            legacy.ActiveAccessories,
            new List<string>() {Constants.Pyjamas},
            "2.2.0");
    }

    public static RecipeDescriptor23 ToVersion230(this RecipeDescriptor22 legacy)
    {
        return new RecipeDescriptor23(
            legacy.AnimatorSource,
            legacy.BaseOutfitName,
            legacy.BaseOutfitName,
            legacy.ActiveAccessories,
            legacy.ActiveEffects,
            "2.3.0");
    }
}
