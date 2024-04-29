using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Utils;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Recipes;
internal class AutoSaver
{

    OutfitManager outfitManager;

    public AutoSaver(PlayerCarolInstance player)
    {
        outfitManager = player.outfitManager;
        OutfitAssetManager.OnOutfitSetLoaded += Load;
    }

    public void Save()
    {
        var descriptor = new RecipeDescriptor21(outfitManager);
        var path = RecipeSaver.RecipeFilenameToPath(Constants.AutoSave);
        RecipeSaver.Save(descriptor, path);
    }

    public void Load()
    {
        var recipe = CCPlugin.recipesManager.GetRecipeByFilename(Constants.AutoSave);
        CCPlugin.uiInstances.First().StartCoroutine(LoadRecipeRoutine(recipe));//TODO: put this on a proper gameobject
    }

    private IEnumerator LoadRecipeRoutine(Recipe recipe)
    {
        if (!outfitManager.pelvis) yield return new WaitUntil(() => outfitManager.pelvis);
        if (recipe is null 
            || recipe.Error == Recipe.Status.FileError
            || recipe.Error == Recipe.Status.InvalidJson)
        {
            Log.Warning("Loading pyjamas instead of autosave");
            RecipeApplier.ActivateVariant(outfitManager, Constants.Pyjamas, 0);
            yield break;
        }
        Log.Info("Loading autosave recipe");
        RecipeApplier.ActivateRecipe(outfitManager, recipe.Descriptor);
        Log.Debug("Load completed succesfully");
        yield break;
    }
}
