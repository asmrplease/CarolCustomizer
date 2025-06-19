using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Utils;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Recipes;
internal class AutoSaver : IDisposable
{
    OutfitManager outfitManager;
    readonly int playerIndex;
    readonly string path;

    public AutoSaver(PlayerCarolInstance player, int playerIndex = 0)
    {
        outfitManager = player.outfitManager;
        OutfitAssetManager.OnOutfitSetLoaded += Load;
        this.playerIndex = playerIndex;
        this.path = RecipeSaver.RecipeFilenameToPath(
            Constants.AutoSave 
            + playerIndex 
            + Constants.JsonFileExtension);
    }

    public void Dispose()
    {
        OutfitAssetManager.OnOutfitSetLoaded -= Load;
    }

    public void Save()
    {
        var recipe = new RecipeDescriptor24(outfitManager);
        if (!recipe.ActiveAccessories.Any()) { Log.Warning($"Skipping Player {playerIndex + 1} autosave becuase no accessories are active. "); return; }

        RecipeSaver.SaveJson(
            recipe,
            this.path);
        Log.Info("Autosave Complete.");
    }

    public void Load()
    {
        var recipe = CCPlugin
            .recipesManager
            .GetRecipeByFilename(this.path);
        CCPlugin.CoroutineRunner.StartCoroutine(LoadRecipeRoutine(recipe));
    }

    IEnumerator LoadRecipeRoutine(Recipe recipe)
    {
        if (!outfitManager.pelvis) yield return new WaitUntil(() => outfitManager.pelvis);

        if (recipe is null 
            || recipe.Error == Recipe.Status.FileError
            || recipe.Error == Recipe.Status.InvalidJson
            || !recipe.Descriptor.ActiveAccessories.Any())
        {
            Log.Warning("Loading pyjamas instead of autosave");
            var outfit = OutfitAssetManager.GetPyjamas();
            var variant = outfit
                .Variants
                .Keys
                .ElementAt(playerIndex);
            RecipeApplier.ActivateVariant(outfitManager, outfit, variant);
            yield break;
        }
        Log.Info("Loading autosave recipe");
        RecipeApplier.ActivateRecipe(outfitManager, recipe.Descriptor);
        Log.Debug("Load completed succesfully");
        yield break;
    }
}
