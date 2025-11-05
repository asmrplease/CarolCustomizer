using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using Onirism.Gameplay;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Assets;
internal class HaDSOutfitLoader : IDisposable
{
    static string ListName = "HaDS Outfits";
    static Dictionary<SourceDescriptor, HaDSOutfit> HaDSOutfits = new();

    public HaDSOutfitLoader()
    {
        OutfitAssetManager.outfitSets.Add(ListName, HaDSOutfits);
    }

    public IEnumerator LoadAllHaDSOutfits()
    {
        //var reference = CCPlugin.uiInstance.loadingIndicator.NotifyLoadingStart();
        Log.Info("Loading hair");
        LoadHair();
        Log.Info("Hair load complete, loading outfits");
        var list = Resources.FindObjectsOfTypeAll<ModelData>();

        var vanillaOutfits = list.Where(x =>
            x.gameObject.name.StartsWith("carol_", StringComparison.InvariantCultureIgnoreCase)
            && !x.gameObject.name.Contains("(Clone)"));//this line is designed to filter for any outfits that might be active when plugin is loaded 

        foreach (var outfit in vanillaOutfits)
        {
            LoadHaDSOutfit(outfit);
            yield return null;
        }

        OutfitAssetManager.OnOutfitSetLoaded?.Invoke();
        //CCPlugin.uiInstance.loadingIndicator.NotifyLoadingComplete(reference);
    }

    void LoadHair()
    {
        var hair = Resources.FindObjectsOfTypeAll<Hairstyle>()
            .Where(x => !x.gameObject.name.Contains("(Clone)"))
            .Select(x => new StoredHair(x))
            .ToList();
        //var colors = Resources.FindObjectsOfTypeAll<Material>()
        //    .Where(x => x.name.StartsWith("CRL") && !x.name.Contains("(Instance)"))
        //    .ToList();
        var dyes = Resources.FindObjectsOfTypeAll<HairDye>()
            .ToList();
        OutfitAssetManager.NotifyHairReady(hair, dyes);
    }

    void LoadHaDSOutfit(ModelData outfit)
    {
        var hads = new HaDSOutfit(outfit.transform);
        if (!HaDSOutfits.TryAdd(hads.Descriptor, hads)) return;

        OutfitAssetManager.OnOutfitLoaded?.Invoke(hads);
    }

    public void Dispose()
    {
        HaDSOutfits.Values.ForEach(x => x.Dispose());
        //foreach (var outfit in HaDSOutfits.Values) { outfit.Dispose(); }
    }

}
