using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using static CarolCustomizer.Models.Materials.MaterialDescriptor;

namespace CarolCustomizer.Behaviors.Recipes;
public static class RecipeApplier
{
    public static void ActivateRecipe(OutfitCoordinator target, RecipeDescriptor recipe)
    {
        target.DisableAllAccessories();
        target.DisableAllEffects();
        //load hair first so if it's replaced later, we don't overwrite it. 
        target.SetHairstyle(OutfitAssetManager.GetHairstyle(recipe.Hairstyle));
        target.SetHairColor(OutfitAssetManager.GetHairColorMaterial(recipe.HairMaterial));
        recipe
            .ActiveAccessories
            .ForEach(x => SetAccessory(target, x));
        CCPlugin.CoroutineRunner.StartCoroutine(SceneResourceProvider.BatchLoad());
        recipe
            .ActiveEffects
            .Select(OutfitAssetManager.GetOutfitByAssetName)
            .Where(x => x is not null)
            .ForEach(x => 
                target
                .SetEffect(x, true));
        target.SetAnimator(
            OutfitAssetManager.GetOutfitByAssetName(recipe.AnimatorSource));
        target.SetConfiguration(
            OutfitAssetManager.GetOutfitByAssetName(recipe.BaseOutfitName));
        target.SetColliderSource(
            OutfitAssetManager.GetOutfitByAssetName(recipe.ColliderSource));
    }

    public static void ActivateVariant(OutfitCoordinator outfitManager, HaDSOutfit outfit, string variantName)
    {
        Log.Debug("ActivateVariant()");
        if (outfitManager is null) { Log.Error("RecipeApplier.ActivateVariant() null outfit manager"); return; }
        if (outfit is null) { Log.Error("RecipeApplier.ActivateVariant() null outfit"); return; }

        outfitManager.DisableAllAccessories();
        outfitManager.DisableAllEffects();

        outfit.Variants.TryGetValue(variantName, out var variantAccs);
        if (variantAccs is null) { Log.Warning($"variant {variantName} not found in {outfit.DisplayName}"); return; }

        outfitManager.SetHairstyle(outfit.modelData.defaultHairstyle.GetComponent<Hairstyle>());
        outfitManager.SetHairColor(outfit.modelData.defaultHaircolor);
        variantAccs.ForEach(x => SetAccessory(outfitManager, x));
        outfitManager.SetEffect(outfit, true);
        outfitManager.SetAnimator(outfit);
        outfitManager.SetConfiguration(outfit);
        outfitManager.SetColliderSource(outfit);
    }

    public static void ActivateFirstVariant(OutfitCoordinator outfitManager, string outfitName)
    {
        Log.Debug("ActivateVariant(OM, string, int");
        
        HaDSOutfit outfit = OutfitAssetManager.GetOutfitByAssetName(outfitName);
        if (outfit is null) { Log.Debug($"Didn't find outfit named: {outfitName}"); return; }
            
        var variant = outfit.Variants.First().Key;
        ActivateVariant(outfitManager, outfit, variant);
    }

    static void SetAccessory(OutfitCoordinator target, AccessoryDescriptor accessoryDescription)
    {
        Log.Debug($"Setting accessory {accessoryDescription.Source}:{accessoryDescription.Name}...");
        var accessory = OutfitAssetManager.GetAccessory(accessoryDescription);

        if (accessory is null && accessoryDescription.Name.ToLower().Contains("hair"))
        {
            Log.Info("hair accessory was missing, setting hairstyle to outfit's hair.");
            var outfit = accessory.outfit as HaDSOutfit;
            target.SetHairstyle(outfit.modelData.defaultHairstyle.GetComponent<Hairstyle>());
            target.SetHairColor(outfit.modelData.defaultHaircolor);
            return;
        }
        else if (accessory is null) { Log.Warning($"failed to find accessory {accessoryDescription.Name}"); return; }

        target.EnableAccessory(accessory);
        accessoryDescription
            .Materials
            .Select((mat, index) => (mat, index))
            .ForEach(tup => SetMaterial(target, accessory, tup.mat, tup.index));
    }

    static void SetMaterial(OutfitCoordinator target, StoredAccessory accessory, MaterialDescriptor materialDescription, int index)
    {
        Log.Debug("setting material...");
        if (materialDescription.Type == MaterialDescriptor.SourceType.World)
        {
            Log.Info("Attempting to load world material");
            CCPlugin
                .CoroutineRunner
                .StartCoroutine
                (
                    SceneResourceProvider.BatchQueueAndThen(
                        materialDescription, 
                        (x) => target.PaintAccessory(accessory, x, index))
                );
        }
        else if (materialDescription.Type == MaterialDescriptor.SourceType.AssetBundle)
        {
            var outfit = OutfitAssetManager.GetOutfitByAssetName(materialDescription.Source);
            if (outfit is null) { Log.Warning($"failed to find {materialDescription.Source}."); return; }
            Log.Debug("found outfit again...");
            if (outfit.MaterialDescriptors is null) { Log.Warning($"failed to get {materialDescription.Source} MaterialDescriptors"); return; }

            outfit.MaterialDescriptors.TryGetValue(materialDescription, out var liveMaterial);
            if (liveMaterial is null) { Log.Warning($"failed to find {materialDescription.Name} in source {materialDescription.Source}"); return; }

            target.PaintAccessory(accessory, liveMaterial, index);
        }
        else if (materialDescription.Type == MaterialDescriptor.SourceType.Resources)
        {
            //we shouldn't be saving materials of this type probably idk lol
        }
        
        
    }

    public static IEnumerable<(string, SourceType)> GetMatSources(RecipeDescriptor recipe)
    {
        var matSources = recipe
            .ActiveAccessories
            .SelectMany(x => x.Materials)
            .Select(x => (x.Source, x.Type))
            .Distinct();
        return matSources;
    }

    public static IEnumerable<MaterialDescriptor> GetWorldMats(RecipeDescriptor recipe)
    {
        return recipe
            .ActiveAccessories
            .SelectMany(x => x.Materials)
            .Where(x => x.Type == SourceType.World)
            .Distinct();
    }

    public static IEnumerable<string> GetSources(RecipeDescriptor recipe)
    {
        var accSources = recipe
            .ActiveAccessories
            .Select(x => x.Source);
        var matSources = recipe
            .ActiveAccessories
            .SelectMany(x => x.Materials)
            .Where(x => x.Type != SourceType.World)
            .Select(x => x.Source);
        return accSources
            .Concat(matSources)
            .Concat(recipe.ActiveEffects)
            .AddItem(recipe.AnimatorSource)
            .AddItem(recipe.BaseOutfitName)
            .AddItem(recipe.ColliderSource)
            .Distinct();
    }

    public static IEnumerable<string> GetMissingSources(RecipeDescriptor recipe)
    {
        return GetSources(recipe).Where(x => OutfitAssetManager.GetOutfitByAssetName(x) is null);
    }

}
