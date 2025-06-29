﻿using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace CarolCustomizer.Behaviors.Recipes;
public static class RecipeApplier
{
    public static void ActivateRecipe(OutfitManager target, LatestDescriptor recipe)
    {
        target.DisableAllAccessories();
        target.DisableAllEffects();
        //load hair first so if it's replaced later, we don't overwrite it. 
        target.SetHairstyle(OutfitAssetManager.GetHairstyle(recipe.Hairstyle));
        target.SetHairColor(OutfitAssetManager.GetHairColor(recipe.HairMaterial));
        recipe
            .ActiveAccessories
            .ForEach(x => SetAccessory(target, x));
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

    public static void ActivateVariant(OutfitManager outfitManager, HaDSOutfit outfit, string variantName)
    {
        Log.Debug("ActivateVariant()");
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

    public static void ActivateFirstVariant(OutfitManager outfitManager, string outfitName)
    {
        Log.Debug("ActivateVariant(OM, string, int");
        
        HaDSOutfit outfit = OutfitAssetManager.GetOutfitByAssetName(outfitName);
        if (outfit is null) { Log.Debug($"Didn't find outfit named: {outfitName}"); return; }
            
        var variant = outfit.Variants.First().Key;
        ActivateVariant(outfitManager, outfit, variant);
    }

    static void SetAccessory(OutfitManager target, AccessoryDescriptor accessoryDescription)
    {
        Log.Debug($"Setting accessory {accessoryDescription.Source}:{accessoryDescription.Name}...");
        var outfit = OutfitAssetManager.GetOutfitByAssetName(accessoryDescription.Source);
        if (outfit is null) { Log.Warning($"failed to find outfit {accessoryDescription.Source}."); return; }
        
        Log.Debug("found source");
        var accessory = outfit.GetAccessory(accessoryDescription);
        if (accessory is null && accessoryDescription.Name.ToLower().Contains("hair"))
        {
            Log.Info("hair accessory was missing, setting hairstyle to outfit's hair.");
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

    static void SetMaterial(OutfitManager target, StoredAccessory accessory, MaterialDescriptor materialDescription, int index)
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

        target.PaintAccessory(accessory, liveMaterial, index);
    }

    public static IEnumerable<string> GetSources(LatestDescriptor recipe)
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

    public static IEnumerable<string> GetMissingSources(LatestDescriptor recipe)
    {
        return GetSources(recipe)
            .Where(x => OutfitAssetManager.GetOutfitByAssetName(x) is null);
    }

}
