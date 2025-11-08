using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Models;
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
    public static void ActivateRecipe(OutfitCoordinator target, RecipeDescriptor recipe)
    {
        target.DisableAllAccessories();
        target.DisableAllEffects();

        recipe
            .ActiveAccessories
            .ForEach(x => SetAccessory(target, x));
        CCPlugin.CoroutineRunner.StartCoroutine(SceneResourceProvider.BatchLoad());
        recipe
            .ActiveEffects
            .ForEach(x => target.SetEffect(x, true));
        target.SetAnimator(recipe.AnimatorSource);
        target.SetConfiguration(recipe.ConfigurationSource);
        target.SetColliderSource(recipe.ColliderSource);
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

        var hairstyle = outfit.modelData.defaultHairstyle.GetComponent<Hairstyle>();
        variantAccs.ForEach(x => SetAccessory(outfitManager, x));
        outfitManager.SetEffect(outfit.Descriptor, true);
        outfitManager.SetAnimator(outfit.Descriptor);
        outfitManager.SetConfiguration(outfit.Descriptor);
        outfitManager.SetColliderSource(outfit.Descriptor);
    }

    public static void ActivateFirstVariant(OutfitCoordinator outfitManager, string outfitName)
    {
        Log.Debug("ActivateVariant(OM, string, int");
        HaDSOutfit outfit = OutfitAssetManager.GetOutfitByAssetName(outfitName);
        if (outfit is null) { Log.Debug($"Didn't find outfit named: {outfitName}"); return; }
            
        var variant = outfit.Variants.First().Key;
        ActivateVariant(outfitManager, outfit, variant);
    }

    static void SetAccessory(OutfitCoordinator target, AccessoryDescriptor acc)
    {
        Log.Debug($"Setting accessory {acc.Source}:{acc.Name}");
        target.EnableAccessory(acc);
        acc
            .Materials
            .Select((mat, index) => (mat, index))
            .ForEach(tup => SetMaterial(target, acc, tup.mat, tup.index));
    }

    static void SetMaterial(OutfitCoordinator target, AccessoryDescriptor accessory, MaterialDescriptor materialDescription, int index)
    {
        Log.Debug("setting material...");
        if (materialDescription.Source.Type == SourceType.World)
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
        else if (materialDescription.Source.Type == SourceType.Outfit)
        {
            var source = OutfitAssetManager.GetAccessorySource(materialDescription.Source);
            if (source is null) { Log.Warning($"failed to find {materialDescription.Source}."); return; }
            Log.Debug("found outfit again...");

            var liveMaterial = source.GetMaterial(materialDescription);
            if (liveMaterial is null) { Log.Warning($"failed to find {materialDescription.Name} in source {materialDescription.Source}"); return; }

            target.PaintAccessory(accessory, liveMaterial, index);
        }
        else if (materialDescription.Source.Type == SourceType.Resources)
        {
            //we shouldn't be saving materials of this type probably idk lol
        }
    }

    public static IEnumerable<SourceDescriptor> GetMatSources(RecipeDescriptor recipe)
    {
        var matSources = recipe
            .ActiveAccessories
            .SelectMany(x => x.Materials)
            .Select(x => x.Source)
            .Distinct();
        return matSources;
    }

    public static IEnumerable<MaterialDescriptor> GetWorldMats(RecipeDescriptor recipe)
    {
        return recipe
            .ActiveAccessories
            .SelectMany(x => x.Materials)
            .Where(x => x.Source.Type == SourceType.World)
            .Distinct();
    }

    public static IEnumerable<SourceDescriptor> GetSources(RecipeDescriptor recipe)
    {
        var accSources = recipe
            .ActiveAccessories
            .Select(x => x.Source);
        var matSources = recipe
            .ActiveAccessories
            .SelectMany(x => x.Materials)
            .Where(x => x.Source.Type != SourceType.World)
            .Select(x => x.Source);
        return accSources
            .Concat(matSources)
            .Concat(recipe.ActiveEffects)
            .AddItem(recipe.AnimatorSource)
            .AddItem(recipe.ConfigurationSource)
            .AddItem(recipe.ColliderSource)
            .Distinct();
    }

    public static IEnumerable<SourceDescriptor> GetMissingSources(RecipeDescriptor recipe)
    {
        return GetSources(recipe)
            .Where(x => OutfitAssetManager.GetAccessorySource(x) is null);
    }

    public static IEnumerable<AccessoryDescriptor> GetRemovedAccessories(RecipeDescriptor recipe)
    {
        return recipe.ActiveAccessories
            .Select(acc => (acc, source: OutfitAssetManager.GetAccessorySource(acc.Source)))
            .Where(tup => tup.source is not null)
            .Where(tup => tup.source.GetInstantiable(tup.acc) is null)
            .Select(tup => tup.acc);
    }

    //public static IEnumerable<AccessoryDescriptor> GetAllMissingAccessories(RecipeDescriptor recipe)
    //{
        
    //    return recipe.ActiveAccessories

    //}

}
