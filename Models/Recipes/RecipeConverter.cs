using System;
using System.Collections.Generic;
using System.Text;

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
}
