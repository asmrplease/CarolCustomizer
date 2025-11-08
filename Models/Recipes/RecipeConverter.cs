using CarolCustomizer.Assets;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using System;
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
        Log.Debug("RecipeDescriptor24 to RecipDescriptor25");
        var accessories = legacy.ActiveAccessories
            .Select(x => (AccessoryDescriptor)x)
            .ToList();
        var effects = legacy.ActiveEffects
            .Select(x => (SourceDescriptor)x);
        var hair = GetHairFromStrings(legacy.Hairstyle, legacy.HairMaterial);
        return new RecipeDescriptor25
        (
            (SourceDescriptor)legacy.AnimatorSource,
            (SourceDescriptor)legacy.BaseOutfitName,
            (SourceDescriptor)legacy.ColliderSource,
            accessories.Append(hair),
            effects,
            "2.5.0"
        );
    }

    internal static AccessoryDescriptor GetHairFromStrings(string hairstyle, string dye)
    {
        var hairSourceDescriptor = new SourceDescriptor(hairstyle, SourceType.Hair);
        var hair = new AccessoryDescriptor
        (
            hairstyle,
            hairSourceDescriptor,
            []
        );
        var request = OutfitAssetManager.GetAccessorySource(hairSourceDescriptor);
        if (request is StoredHair hairSource)
        {
            MaterialDescriptor[] mats = [];
            if (dye != "CRLH_Default_Brown")
            {
                int index = hairSource.hairstyle.mainMaterialIndex;
                mats = hairSource.smr.materials
                    .Select(x => new MaterialDescriptor(x, hairSourceDescriptor))
                    .ToArray();
                mats[index] = new MaterialDescriptor
                (
                    dye,
                    new SourceDescriptor(Constants.HairDyeSourceName, SourceType.Hair)
                );
            }
            hair.Materials = mats;
        }
        else { Log.Warning($"failed to find hair source {hairSourceDescriptor}"); }

        return hair;
    }

}
