using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.UI.Recipes;
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

    public void Load() => CCPlugin.CoroutineRunner.StartCoroutine(LoadRecipeRoutine());

    IEnumerator LoadRecipeRoutine()
    {
        var recipe = CCPlugin
            .recipesManager
            .GetRecipeByFilename(this.path);
        if (!outfitManager.pelvis) yield return new WaitUntil(() => outfitManager.pelvis);

        RecipeDescriptor descriptor;
        if (recipe is null
            || recipe.Error == Recipe.Status.FileError
            || recipe.Error == Recipe.Status.InvalidJson
            || !recipe.Descriptor.ActiveAccessories.Any())
        {
            Log.Warning("Loading pyjamas instead of autosave");
            descriptor = OutfitAssetManager.GetPyjamas()
                .Variants
                .ElementAt(playerIndex)
                .Value;
        }
        else { descriptor = recipe.Descriptor; }
        RecipeApplier.ActivateRecipe(outfitManager, descriptor);
        Log.Debug("Load completed succesfully");
        yield break;
    }
}
