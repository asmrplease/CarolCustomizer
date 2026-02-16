using CarolCustomizer.Assets;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Linq;

namespace CarolCustomizer.Behaviors.Recipes;
public class PrioritySources
{
    static HashSet<SourceDescriptor> SourceList = [];
    static HashSet<string> StringList = [];
    static bool complete = false;

    public static void OnStart()
    {
        Log.Debug("PrioritySources.OnStart()");
        var path = RecipeSaver.RecipeFilenameToPath(
            Constants.AutoSave
            + 0
            + Constants.JsonFileExtension);
        var autosaveFile = CCPlugin
            .recipesManager
            .GetRecipeByFilename(path);
        List<SourceDescriptor> all = [Constants.PyjamaDescriptor];

        if (autosaveFile is not null)
        {
            var descriptor = autosaveFile.Descriptor;
            var active = descriptor.ActiveAccessories
                .Select(x => x.Source);
            all.AddRange(active);
            var mats = descriptor.ActiveAccessories
                .SelectMany(x => x.Materials)
                .Select(x => x.Source);
            all.AddRange(mats);
            all.AddRange(descriptor.ActiveEffects);
            all.Add(descriptor.ConfigurationSource);
            all.Add(descriptor.AnimatorSource);
            all.Add(descriptor.ColliderSource);
        }
        
        var distinct = all
            .Where(x => x.Type == Models.SourceType.Outfit)
            .Distinct();
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

    public static bool IsLowPriority(SourceDescriptor source) => !SourceList.Contains(source);

    public static bool IsLowPriority(string assetName)
    {
        var formatted = assetName.ToLower().Trim();
        var found = StringList.Contains(formatted);
        return !found;
    }
}
