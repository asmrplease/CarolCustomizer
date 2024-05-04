using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Assets;
internal class HaDSOutfitLoader : IDisposable
{
    static string ListName = "HaDS Outfits";
    static Dictionary<string, HaDSOutfit> HaDSOutfits = new();

    public HaDSOutfitLoader()
    {
        OutfitAssetManager.outfitSets.Add(ListName, HaDSOutfits);
    }

    public IEnumerator LoadAllHaDSOutfits()
    {
        Log.Info("loading vanilla outfits");
        var list = Resources.FindObjectsOfTypeAll<ModelData>();

        var vanillaOutfits = list.Where(x =>
            x.gameObject.name.ToLower().StartsWith("carol_")
            && !x.gameObject.name.Contains("(Clone)"));//this line is designed to filter for any outfits that might be active when plugin is loaded

        foreach (var outfit in vanillaOutfits)
        {
            LoadHaDSOutfit(outfit);
            yield return null;
        }

        OutfitAssetManager.OnOutfitSetLoaded?.Invoke();
    }

    void LoadHaDSOutfit(ModelData outfit)
    {
        if (outfit.GetComponentInChildren<PelvisWatchdog>()) { return; }

        var hads = new HaDSOutfit(outfit.transform);
        HaDSOutfits.Add(hads.AssetName, hads);
        OutfitAssetManager.OnOutfitLoaded?.Invoke(hads);
    }

    public void Dispose()
    {
        foreach (var outfit in HaDSOutfits) { outfit.Value.Dispose(); }
    }

}
