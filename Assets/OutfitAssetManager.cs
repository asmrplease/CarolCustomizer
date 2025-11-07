using CarolCustomizer.Contracts;
using CarolCustomizer.Models;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using Onirism.Gameplay;
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
    public static event Action<(List<StoredHair>, List<HairDye>)> OnHairLoaded;

    public static Dictionary<string, Dictionary<SourceDescriptor, HaDSOutfit>> outfitSets = new();
    public static List<StoredHair> Hairstyles = new();
    public static Dictionary<string, HairDye> HairColors = new();

    public OutfitAssetManager(Transform parent)
    {
        liveFolder = new GameObject().transform;
        liveFolder.name = "Customizer Dynamic Assets";
        liveFolder.transform.parent = parent;
        liveFolder.position = Constants.OutOfTheWay;
    }

    public static HaDSOutfit GetPyjamas() => GetOutfitByAssetName(Constants.Pyjamas);

    public static HaDSOutfit GetOutfitByAssetName(string assetName, SourceType type = SourceType.Outfit)
    {
        if (outfitSets is null) { Log.Error("outfitSets was null when searching for asset"); return null; }
        if (assetName is null) { Log.Warning("GetOutfitByAssetName was given a null value"); return null; }

        return outfitSets.Values
            .Select(dict =>
                (found: dict.TryGetValue(new SourceDescriptor(assetName, type), out var result)
                , result: result))
            .FirstOrDefault(tup => tup.found)
            .result;
    }

    public static IAccessorySource GetAccessorySource(SourceDescriptor descriptor)
    {
        //return GetOutfitSource(descriptor);
        return descriptor.Type switch
        {
            SourceType.Outfit => GetOutfitSource(descriptor),
            SourceType.Hair => GetHairSource(descriptor),
            SourceType.World => GetWorldSource(descriptor),
            SourceType.Resources => null,
            _ => null,
        };
    }

    static IAccessorySource GetOutfitSource(SourceDescriptor descriptor)
    {
        var idk = outfitSets.Values
            .Select(dict =>
                (found: dict.TryGetValue(descriptor, out var result)
                , result: result))
            .FirstOrDefault(tup => tup.found)
            .result;
        if (idk is not null) return idk;

        Log.Warning($"No source of type {descriptor.Type} named {descriptor.Name} was found.");
        return null;
    }

    static IAccessorySource GetHairSource(SourceDescriptor descriptor)
    {
        throw new NotImplementedException();
    }

    static IAccessorySource GetWorldSource(SourceDescriptor descriptor)
    {
        throw new NotImplementedException();
    }

    public static StoredAccessory GetAccessory(AccessoryDescriptor descriptor)
    {
        //var outfit = GetOutfitByAssetName(descriptor.Source);
        var source = GetAccessorySource(descriptor.Source);
        if (source is null) { Log.Warning($"failed to find outfit {descriptor.Source}."); return null; }

        Log.Debug("found source");
        return source.GetAccessory(descriptor);
    }

    public static StoredHair GetHairstyle(string name)
    {
        var hair = Hairstyles.FirstOrDefault(x => x.AssetName == name);
        if (hair is null) Log.Warning($"When searching for hairstyle '{name}', no results were found. ");
        return hair;
    }

    public static Material GetHairColorMaterial(string assetName)
    {
        if (!HairColors.TryGetValue(assetName, out var style)) { Log.Warning($"failed to find material named {assetName}"); return null; }
        Log.Debug($"requested {assetName}, found hair dye{style.name}");
        return style.material;
    }

    public static void NotifyHairReady(List<StoredHair> hair, List<HairDye> dye)
    {
        Hairstyles.AddRange(hair);
        //TODO: this will fail if we ever load more hair dye colors
        HairColors = dye.ToDictionary(x => x.locKey, x => x);
        OnHairLoaded?.Invoke((hair, dye));
    }

    public void Dispose() { if (liveFolder.gameObject) GameObject.Destroy(liveFolder.gameObject); }
}
