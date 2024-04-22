using CarolCustomizer.Models;
using System.Collections.Generic;
using System.Linq;
using CarolCustomizer.Utils;
using CarolCustomizer.Assets;
using HarmonyLib;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Behaviors.Carol;

namespace CarolCustomizer.Behaviors.Recipes;
internal static class RecipeApplier
{
    public static void ActivateRecipe(OutfitManager outfitManager, RecipeDescriptor21 recipe)
    {
        outfitManager.HideBase();
        outfitManager.DisableAllAccessories();
        foreach (var accessory in recipe.ActiveAccessories) { SetAccessory(outfitManager, accessory); }
        outfitManager.SetAnimator(
            OutfitAssetManager
            .GetOutfitByAssetName(
            recipe.AnimatorSource));
    }

    public static void ActivateVariant(OutfitManager outfitManager, HaDSOutfit outfit, string variantName)
    {
        Log.Debug("ActivateVariant()");
        outfitManager.DisableAllAccessories();

        outfit.Variants.TryGetValue(variantName, out var variant);
        if (variant is null) { Log.Warning($"variant {variantName} not found in {outfit.DisplayName}"); return; }

        foreach (var acc in variant) SetAccessory(outfitManager, acc);
    }

    private static void SetAccessory(OutfitManager outfitManager, AccessoryDescriptor accessoryDescription)
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

    private static void SetMaterial(OutfitManager outfitManager, StoredAccessory accessory, MaterialDescriptor materialDescription, int index)
    {
        Log.Debug("setting material...");
        if (materialDescription.Type == MaterialDescriptor.SourceType.World)
        { Log.Warning("idk how to load world materials yet"); return; }

        var outfit = OutfitAssetManager.GetOutfitByAssetName(materialDescription.Source);
        if (outfit is null) { Log.Warning($"failed to find {materialDescription.Source}."); return; }
        Log.Debug("found outfit again...");
        if (outfit.MaterialDescriptors is null) { Log.Warning($"failed to get {materialDescription.Source} MaterialDescriptors"); return; }

        MaterialDescriptor liveMaterial;
        outfit.MaterialDescriptors.TryGetValue(materialDescription, out liveMaterial);
        if (liveMaterial is null) { Log.Warning($"failed to find {materialDescription.Name} in source"); return; }

        outfitManager.PaintAccessory(accessory, liveMaterial, index);
    }

    public static IEnumerable<string> GetSources(RecipeDescriptor21 recipe)
    {
        Log.Debug("GetSources()");
        if (recipe.ActiveAccessories is null) { Log.Error("no accessories in recipe!"); return new List<string>(); };
        var accSources = recipe.ActiveAccessories.Select(x => x.Source);
        var matSources = recipe.ActiveAccessories.SelectMany(x => x.Materials).Select(x => x.Source);
        var animatorSource = recipe.AnimatorSource;
        return accSources.Concat(matSources).AddItem(animatorSource).Distinct();
    }

    public static IEnumerable<string> GetMissingSources(RecipeDescriptor21 recipe)
    {
        return GetSources(recipe).Where(x => OutfitAssetManager.GetOutfitByAssetName(x) is null);
    }
}
