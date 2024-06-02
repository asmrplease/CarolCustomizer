using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Models;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Utils;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace CarolCustomizer.Behaviors.Recipes;
internal static class RecipeApplier
{
    public static void ActivateRecipe(OutfitManager outfitManager, RecipeDescriptor23 recipe)
    {
        outfitManager.DisableAllAccessories();
        outfitManager.DisableAllEffects();

        recipe
            .ActiveAccessories
            .ForEach(x => 
                SetAccessory(outfitManager, x));
        recipe
            .ActiveEffects
            .Select(
                OutfitAssetManager
                .GetOutfitByAssetName)
            .Where(x => x is not null)
            .ForEach(x => 
                outfitManager
                .SetEffect(x, true));
        outfitManager.SetAnimator(
            OutfitAssetManager
            .GetOutfitByAssetName(
            recipe.AnimatorSource));
        outfitManager.SetConfiguration(
            OutfitAssetManager
            .GetOutfitByAssetName(
               recipe.BaseOutfitName));
        outfitManager.SetColliderSource(
            OutfitAssetManager
            .GetOutfitByAssetName(
                recipe.ColliderSource));
    }

    public static void ActivateFirstVariant(OutfitManager outfitManager, string outfitName)
    {
        Log.Debug("ActivateVariant(OM, string, int");
        
        HaDSOutfit outfit = OutfitAssetManager.GetOutfitByAssetName(outfitName);
        if (outfit is null) { Log.Debug($"Didn't find outfit named: {outfitName}"); return; }
            
        var variant = outfit.Variants.First().Key;
        ActivateVariant(outfitManager, outfit, variant);
    }

    public static void ActivateVariant(OutfitManager outfitManager, HaDSOutfit outfit, string variantName)
    {
        Log.Debug("ActivateVariant()");
        outfitManager.DisableAllAccessories();
        outfitManager.DisableAllEffects();

        outfit.Variants.TryGetValue(variantName, out var variant);
        if (variant is null) { Log.Warning($"variant {variantName} not found in {outfit.DisplayName}"); return; }

        foreach (var acc in variant) SetAccessory(outfitManager, acc);
        outfitManager.SetEffect(outfit, true);
        outfitManager.SetAnimator(outfit);
        outfitManager.SetConfiguration(outfit);
        outfitManager.SetColliderSource(outfit);
    }

    static void SetAccessory(OutfitManager outfitManager, AccessoryDescriptor accessoryDescription)
    {
        Log.Debug($"Setting accessory {accessoryDescription.Source}:{accessoryDescription.Name}...");
        var outfit = OutfitAssetManager.GetOutfitByAssetName(accessoryDescription.Source);
        if (outfit is null) { Log.Warning($"failed to find {accessoryDescription.Source}."); return; }
        Log.Debug("found source");

        var accessory = outfit.GetAccessory(accessoryDescription);
        if (accessory is null) { Log.Warning($"failed to find {accessoryDescription.Name}"); return; }
        Log.Debug("found accessory");

        outfitManager.EnableAccessory(accessory);
        Log.Debug("enabled accessory");

        int index = 0;
        foreach (var material in accessoryDescription.Materials)
        {
            SetMaterial(outfitManager, accessory, material, index++);
        }
    }

    static void SetMaterial(OutfitManager outfitManager, StoredAccessory accessory, MaterialDescriptor materialDescription, int index)
    {
        Log.Debug("setting material...");
        if (materialDescription.Type == MaterialDescriptor.SourceType.World)
        { Log.Warning("idk how to load world materials yet"); return; }

        var outfit = OutfitAssetManager.GetOutfitByAssetName(materialDescription.Source);
        if (outfit is null) { Log.Warning($"failed to find {materialDescription.Source}."); return; }
        Log.Debug("found outfit again...");
        if (outfit.MaterialDescriptors is null) { Log.Warning($"failed to get {materialDescription.Source} MaterialDescriptors"); return; }

        outfit.MaterialDescriptors.TryGetValue(materialDescription, out var liveMaterial);
        if (liveMaterial is null) { Log.Warning($"failed to find {materialDescription.Name} in source"); return; }

        outfitManager.PaintAccessory(accessory, liveMaterial, index);
    }

    public static IEnumerable<string> GetSources(RecipeDescriptor23 recipe)
    {
        var accSources = recipe
            .ActiveAccessories
            .Select(x => x.Source);
        var matSources = recipe
            .ActiveAccessories
            .SelectMany(x => x.Materials)
            .Select(x => x.Source);

        return accSources
            .Concat(matSources)
            .Concat(recipe.ActiveEffects)
            .AddItem(recipe.AnimatorSource)
            .AddItem(recipe.BaseOutfitName)
            .AddItem(recipe.ColliderSource)
            .Distinct();
    }

    public static IEnumerable<string> GetMissingSources(RecipeDescriptor23 recipe)
    {
        return GetSources(recipe)
            .Where(x => 
                OutfitAssetManager
                .GetOutfitByAssetName(x) 
                is null);
    }
}
