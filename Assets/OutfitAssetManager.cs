using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Assets;
public class OutfitAssetManager : IDisposable
{
    public static Transform liveFolder { get; private set; }
    //TODO: make these Events
    public static Action<Outfit> OnOutfitLoaded;
    public static Action<Outfit> OnOutfitUnloaded;
    public static Action OnOutfitSetLoaded;
    public static Action OnOutfitSetUnloaded;
    public static event Action<(List<Hairstyle>, List<Material>)> OnHairLoaded;

    public static Dictionary<string, Dictionary<string, HaDSOutfit>> outfitSets = new();
    public static List<Hairstyle> Hairstyles = new();
    public static List<Material> HairColors = new();

    public OutfitAssetManager(Transform parent)
    {
        liveFolder = new GameObject().transform;
        liveFolder.name = "Customizer Dynamic Assets";
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
                , result: result))
            .FirstOrDefault(tup => tup.found)
            .result;
    }

    public static StoredAccessory GetAccessory(AccessoryDescriptor descriptor)
    {
        var outfit = GetOutfitByAssetName(descriptor.Source);
        if (outfit is null) { Log.Warning($"failed to find outfit {descriptor.Source}."); return null; }

        Log.Debug("found source");
        return outfit.GetAccessory(descriptor);
    }

    public static Hairstyle GetHairstyle(string name)
    {
        var hair = Hairstyles.FirstOrDefault(x => x.name == name);
        if (!hair) Log.Warning($"When searching for hairstyle '{name}', no results were found. ");
        return hair;
    }

    public static Material GetHairColor(string name)
    {
        var style = HairColors.FirstOrDefault(x => x.name == name);
        if (!style) Log.Warning($"When searching for hairstyle '{name}', no results were found. ");
        return style;
    }

    public static void NotifyHairReady(List<Hairstyle> hair, List<Material> colors)
    {
        Hairstyles.AddRange(hair);
        HairColors.AddRange(colors);
        OnHairLoaded?.Invoke((hair, colors));
    }

    public void Dispose() { if (liveFolder.gameObject) GameObject.Destroy(liveFolder.gameObject); }
}
