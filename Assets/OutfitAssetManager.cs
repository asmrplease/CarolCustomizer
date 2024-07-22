using System;
using System.Collections.Generic;
using UnityEngine;
using CarolCustomizer.Utils;
using CarolCustomizer.Models.Outfits;
using System.Linq;

namespace CarolCustomizer.Assets;
public class OutfitAssetManager : IDisposable
{
    public static Transform liveFolder { get; private set; }
    //TODO: make these Events
    public static Action<Outfit> OnOutfitLoaded;
    public static Action<Outfit> OnOutfitUnloaded;
    public static Action OnOutfitSetLoaded;
    public static Action OnOutfitSetUnloaded;
    public static Dictionary<string, Dictionary<string, HaDSOutfit>> outfitSets = new();

    public OutfitAssetManager(Transform parent)
    {
        liveFolder = new GameObject().transform;
        liveFolder.name = "AccMod Dynamic Assets";
        liveFolder.transform.parent = parent;
        liveFolder.position = Constants.OutOfTheWay;
    }

    public static HaDSOutfit GetPyjamas() => GetOutfitByAssetName(Constants.Pyjamas);

    public static HaDSOutfit GetOutfitByAssetName(string assetName)
    {
        if (outfitSets is null) { Log.Error("outfitSets was null when searching for asset"); return null; }
        if (assetName is null) { Log.Warning("GetOutfitByAssetName was given a null value"); return null; }

        return outfitSets.Values
            .Select(dict =>
                (found: dict.TryGetValue(assetName, out var result)
                ,result: result))
            .FirstOrDefault(tup => tup.found)
            .result;
        foreach (var dict in outfitSets.Values) 
        {
            if (dict.TryGetValue(assetName, out var result)) { return result; }
        }
        Log.Warning($"{assetName} was not found in any outfit set");
        return null;
    }

    public void Dispose() => GameObject.Destroy(liveFolder);
}
