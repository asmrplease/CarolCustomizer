using CarolCustomizer.Assets;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Linq;

namespace CarolCustomizer.Behaviors.Recipes;
public class PrioritySources
{
    static RecipeDescriptor Autosave;
    static HashSet<SourceDescriptor> SourceList = [];
    static HashSet<string> StringList = [];
    static bool complete = false;

    public static void OnStart()
    {
        Log.Debug("PrioritySources.OnStart()");
        //load json for autosave
        var path = RecipeSaver.RecipeFilenameToPath(
            Constants.AutoSave
            + 0
            + Constants.JsonFileExtension);
        var autosaveRecipe = CCPlugin
            .recipesManager
            .GetRecipeByFilename(path);
        if (autosaveRecipe is null) { Log.Info("No autosave recipe available during PrioritySources.OnStart()"); return; }

        Autosave = autosaveRecipe.Descriptor;
        Log.Debug("Descriptor loaded");
        //build source list from recipedescriptor
        var all = Autosave.ActiveAccessories
            .Select(x => x.Source)
            .ToList();
        var mats = Autosave.ActiveAccessories
            .SelectMany(x => x.Materials)
            .Select(x => x.Source);
        all.AddRange(mats);
        all.AddRange(Autosave.ActiveEffects);
        all.Add(Autosave.ConfigurationSource);
        all.Add(Autosave.AnimatorSource);
        all.Add(Autosave.ColliderSource);
        all.Add(Constants.PyjamaDescriptor);
        var distinct = all
            .Where(x => x.Type == Models.SourceType.Outfit)
            .Distinct();
        //publish list for source loaders to use to prioritize
        SourceList = distinct.ToHashSet();
        StringList = distinct
            .Where(x => x.Name.Contains('_'))
            .Select(x => x.Name.Split('_')[1].ToLower().Trim())
            .ToHashSet();
        StringList.ForEach(Log.Debug);
        Log.Debug("Lists created");
        OutfitAssetManager.OnOutfitLoaded += OnOutfitLoaded;
        Log.Debug("PrioritySources.OnStart() Complete");
    }

    static void OnOutfitLoaded(Outfit outfit)
    {
        Log.Debug("PrioritySources.OnOutfitLoaded()");
        Log.Debug($"still waiting for {SourceList.Count()} autosave sources to load");
        if (!SourceList.Contains(outfit.Descriptor)) return;

        var str = outfit.Descriptor.Name.Split('_')[1].ToLower();
        SourceList.Remove(outfit.Descriptor);
        StringList.Remove(str);
        if (SourceList.Any()) { Log.Debug($"still waiting for {SourceList.Count()} autosave sources to load"); return; }

        Log.Info("Autosave recipe ready");
        PlayerInstances.DefaultPlayer.autoSaver.Load();
        OutfitAssetManager.OnOutfitLoaded -= OnOutfitLoaded;
    }

    public static bool IsLowPriority(SourceDescriptor source)
    {
        return !SourceList.Contains(source);
    }

    public static bool IsLowPriority(string assetName)
    {
        var formatted = assetName.ToLower().Trim();
        var found = StringList.Contains(formatted);
        //if (found) { Log.Info($"{assetName} formatted to {formatted} was in the list"); }
        //else { Log.Debug($"{assetName} formatted to {formatted} was not in the list"); }
        return !found;
    }
}
