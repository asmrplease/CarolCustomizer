using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Linq;

namespace CarolCustomizer.Models.Recipes;

internal static class RecipeConverter
{
    internal static RecipeDescriptor21 ToVersion210(this RecipeDescriptor20 legacy)
    {
        return new RecipeDescriptor21(
            legacy.BaseOutfitName,
            legacy.BaseOutfitName,
            legacy.ActiveAccessories,
            "2.1.0");
    }

    internal static RecipeDescriptor22 ToVersion220(this RecipeDescriptor21 legacy) 
    {
        return new RecipeDescriptor22(
            legacy.AnimatorSource,
            legacy.BaseOutfitName,
            legacy.ActiveAccessories,
            [Constants.Pyjamas],
            "2.2.0");
    }

    internal static RecipeDescriptor23 ToVersion230(this RecipeDescriptor22 legacy)
    {
        return new RecipeDescriptor23(
            legacy.AnimatorSource,
            legacy.BaseOutfitName,
            legacy.BaseOutfitName,
            legacy.ActiveAccessories,
            legacy.ActiveEffects,
            "2.3.0");
    }

    internal static RecipeDescriptor24 ToVersion240(this RecipeDescriptor23 legacy)
    {
        return new RecipeDescriptor24(
            legacy.AnimatorSource,
            legacy.BaseOutfitName,
            legacy.ColliderSource,
            legacy.ActiveAccessories,
            legacy.ActiveEffects,
            "Haircut_Powerhelmet",
            "CRLH_Default_Brown",
            "2.4.0");
    }

    internal static RecipeDescriptor25 ToVersion250(this RecipeDescriptor24 legacy)
    {
        //build hair accessory descriptor
        Log.Warning("i forgot to finish implmenting conversion from old hair config to acc descriptor");
        //AccessoryDescriptor hair = new("",new SourceDescriptor("",SourceType.Hair));
        var accessories = legacy.ActiveAccessories
            .Select(x => (AccessoryDescriptor)x);
            //.Append(hair);
        var effects = legacy.ActiveEffects
            .Select(x => (SourceDescriptor)x);
        return new RecipeDescriptor25
        (
            (SourceDescriptor)legacy.AnimatorSource,
            (SourceDescriptor)legacy.BaseOutfitName,
            (SourceDescriptor)legacy.ColliderSource,
            accessories,
            effects,
            "2.5.0"
        );
    }
}
