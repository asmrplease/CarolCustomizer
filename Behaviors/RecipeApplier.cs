using CarolCustomizer.Contracts;
using CarolCustomizer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CarolCustomizer.Utils;
using CarolCustomizer.Assets;
using HarmonyLib;

namespace CarolCustomizer.Behaviors;
internal static class RecipeApplier
{
    public static void ActivateRecipe(OutfitManager outfitManager, RecipeDescriptor recipe)
    {
        //set the accessory states
        outfitManager.DisableAllAccessories();
        foreach (var accessory in recipe.ActiveAccessories) { SetAccessory(outfitManager, accessory); }

        //set the outfit state last so that we're not immediately trying to change stuff?
        var baseOutfit = OutfitAssetManager.GetOutfitByAssetName(recipe.BaseOutfitName);
        if (baseOutfit is not null) outfitManager.SetBaseOutfit(baseOutfit);
        Log.Debug("Setting base visibility based on recipe state.");
        outfitManager.SetBaseVisibility(recipe.BaseVisible);
    }

    private static void SetAccessory(OutfitManager outfitManager, AccessoryDescriptor accessoryDescription)
    {
        Log.Debug($"Setting accessory {accessoryDescription.Source}:{accessoryDescription.Name}...");
        //find this accessory in the dam 
        var outfit = OutfitAssetManager.GetOutfitByAssetName(accessoryDescription.Source);
        if (outfit is null) { Log.Warning($"failed to find {accessoryDescription.Source}."); return; }
        Log.Debug("found source");

        var accessory = outfit.Accessories.First(x => x.Name == accessoryDescription.Name);
        if (accessory is null) { Log.Warning($"failed to find {accessoryDescription.Name}"); return; }
        Log.Debug("found accessory");

        //call enable accessory with the stored accessory
        outfitManager.EnableAccessory(accessory);
        Log.Debug("enabled accessory");
        //set materials
        int index = 0;
        foreach (var material in accessoryDescription.Materials)
        {
            SetMaterial(outfitManager, accessory, material, index++);
        }
    }

    //TODO: pass the outfit through these methods so we dont' have to keep finding it
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

    public static IEnumerable<string> GetSources(RecipeDescriptor recipe)
    {
        Log.Debug("GetSources()");
        if (recipe.ActiveAccessories is null) { Log.Error("no accessories in recipe!"); return new List<string>(); };
        var accSources = recipe.ActiveAccessories.Select(x => x.Source);
        var matSources = recipe.ActiveAccessories.SelectMany(x => x.Materials).Select(x => x.Source);
        var baseSource = recipe.BaseOutfitName;
        return accSources.Concat(matSources).AddItem(baseSource).Distinct();
    }

    public static IEnumerable<string> GetMissingSources(RecipeDescriptor recipe)
    {
        return GetSources(recipe).Where(x=> OutfitAssetManager.GetOutfitByAssetName(x) is null);
    }
}
