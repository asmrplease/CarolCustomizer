using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Utils;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Recipes;
public class AutoSaver
{
    OutfitCoordinator outfitManager;
    readonly int playerIndex;
    public readonly string path;

    public AutoSaver(PlayerCarolInstance player, int playerIndex = 0)
    {
        outfitManager = player.outfitManager;
        this.playerIndex = playerIndex;
        this.path = RecipeSaver.RecipeFilenameToPath(
            Constants.AutoSave 
            + playerIndex 
            + Constants.JsonFileExtension);
    }

    public void Save()
    {
        var recipe = new RecipeDescriptor(outfitManager);
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

    IEnumerator LoadRecipeRoutine(RecipeFile recipeFile)
    {
        if (!outfitManager.pelvis) yield return new WaitUntil(() => outfitManager.pelvis);
        RecipeDescriptor recipe;

        if (recipeFile is null
            || recipeFile.Error == RecipeFile.Status.FileError
            || recipeFile.Error == RecipeFile.Status.InvalidJson
            || !recipeFile.Descriptor.ActiveAccessories.Any())
        {
            Log.Warning("Loading pyjamas instead of autosave");
            var outfit = OutfitAssetManager.GetPyjamas();
            recipe = outfit
                .Variants
                .ElementAt(playerIndex)
                .Value.Descriptor;
        }
        else 
        { 
            Log.Info("Loading autosave recipe");
            recipe = recipeFile.Descriptor;
        }
        RecipeApplier.ActivateRecipe(outfitManager, recipe);
        Log.Debug("Load completed succesfully");
        yield break;
    }
}
