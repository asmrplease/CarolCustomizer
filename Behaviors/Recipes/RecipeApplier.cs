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
                        (x) => target.PaintAtIndex(accessory, x, index))
                );
            return;
        }

        var mat = OutfitAssetManager.GetMaterial(materialDescription);
        if (mat is null) { Log.Warning($"Failed to find material {materialDescription}"); return; }

        target.PaintAtIndex(accessory, mat, index);
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
        if (recipe is null) return [];
        if (recipe.ActiveAccessories is null) return [];

        return recipe
            .ActiveAccessories
            .Where(x => x is not null)
            .SelectMany(x => x.Materials)
            .Where(x => x is not null && x.Source.Type == SourceType.World)
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
            .Where(x => OutfitAssetManager.GetSource(x, false) is null);
    }

    public static IEnumerable<AccessoryDescriptor> GetRemovedAccessories(RecipeDescriptor recipe)
    {
        return recipe.ActiveAccessories
            .Select(acc => (acc, source: OutfitAssetManager.GetSource(acc.Source, false)))
            .Where(tup => tup.source is not null)
            .Where(tup => tup.source.GetInstantiable(tup.acc) is null)
            .Select(tup => tup.acc);
    }

    //public static IEnumerable<AccessoryDescriptor> GetAllMissingAccessories(RecipeDescriptor recipe)
    //{
        
    //    return recipe.ActiveAccessories

    //}

}
