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
    const string AutoSaveFile = "AutoSave";
    OutfitManager outfitManager;

    public AutoSaver(PlayerCarolInstance player)
    {
        outfitManager = player.outfitManager;
        OutfitAssetManager.OnHaDSOutfitsLoaded += Load;
    }

    public void Save()
    {
        var descriptor = new RecipeDescriptor21(outfitManager);
        var path = RecipeSaver.RecipeFilenameToPath(AutoSaveFile);
        RecipeSaver.Save(descriptor, path);
    }

    public void Load()
    {
        var recipe = CCPlugin.recipesManager.GetRecipeByFilename(AutoSaveFile);
        if (recipe is null) { Log.Warning("Couldn't find autosave recipe"); return; }
        if (recipe.Error != Recipe.Status.NoError) { Log.Warning($"AutoLoad recipe error: {recipe.Error}"); return; }
        CCPlugin.uiInstances.First().StartCoroutine(LoadRoutine(recipe));//TODO: put this on a proper gameobject
    }

    private IEnumerator LoadRoutine(Recipe recipe)
    {
        if (!outfitManager.pelvis) yield return new WaitUntil(() => outfitManager.pelvis);
        RecipeApplier.ActivateRecipe(outfitManager, recipe.Descriptor);
        Log.Debug("Load completed succesfully");
        yield break;
    }
}
