using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CarolCustomizer.Utils;
using CarolCustomizer.Models;
using CarolCustomizer.Hooks;
using System.Collections;
using CarolCustomizer.UI;
using CarolCustomizer.Hooks.Watchdogs;

namespace CarolCustomizer.Assets;
public class OutfitAssetManager : IDisposable
{
    private static string ListName = "HaDS Outfits"; 

    public static Transform liveFolder { get; private set; }
    IntroCutsceneFixBehavior introCutsceneFix;

    public static Action<Outfit> OnOutfitLoaded;
    public static Action<Outfit> OnOutfitUnloaded;
    public static Action OnHaDSOutfitsLoaded;

    static Dictionary<string, HaDSOutfit> HaDSOutfits = new();
    public static Dictionary<string, Dictionary<string, HaDSOutfit>> outfitSets = new();

    public OutfitAssetManager(Transform parent)
    {
        liveFolder = new GameObject().transform;
        liveFolder.name = "AccMod Dynamic Assets";
        liveFolder.transform.parent = parent;
        liveFolder.position = Constants.OutOfTheWay;
        if (!liveFolder) Log.Error("DAM was given a null liveFolder.");

        outfitSets.Add(ListName, HaDSOutfits);

        introCutsceneFix = new(liveFolder.gameObject);
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

        OnHaDSOutfitsLoaded?.Invoke();
    }

    private void LoadHaDSOutfit(ModelData outfit)
    {
        if (outfit.GetComponentInChildren<PelvisWatchdog>()) { return; }

        var hads = new HaDSOutfit(outfit.transform);
        HaDSOutfits.Add(hads.AssetName, hads);
        OnOutfitLoaded?.Invoke(hads);
    }

    public static HaDSOutfit GetOutfitByAssetName(string assetName)
    {
        foreach ((var key, var dict) in outfitSets) 
        {
            if (!dict.ContainsKey(assetName)) { continue; }
            return dict[assetName];
        }
        //Log.Warning($"{assetName} was not found in any ");
        return null;
    }

    public void Dispose()
    {
        foreach (var outfit in HaDSOutfits) { outfit.Value.Dispose(); }
        GameObject.Destroy(liveFolder);
        this.DisposeFields();
    }
}
