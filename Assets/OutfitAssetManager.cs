using CarolCustomizer.Contracts;
using CarolCustomizer.Models;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Materials;
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

    public static Dictionary<string, Dictionary<SourceDescriptor, HaDSOutfit>> outfitSets = [];
    public static Dictionary<SourceDescriptor, StoredHair> Hairstyles = [];
    static HairDyeSource hairDyes;
    public static Dictionary<string, HairDye> HairColors = [];

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
        var result =  descriptor.Type switch
        {
            SourceType.Outfit => GetOutfitSource(descriptor),
            SourceType.Hair => GetHairSource(descriptor),
            SourceType.World => GetWorldSource(descriptor),
            SourceType.Resources => GetWorldSource(descriptor),
            _ => null,
        };

        if (result is null) Log.Warning($"No source of type {descriptor.Type} named {descriptor.Name} was found.");
        return result;
    }

    static IAccessorySource GetOutfitSource(SourceDescriptor descriptor)
    {
        var idk = outfitSets.Values
            .Select(dict =>
                (found: dict.TryGetValue(descriptor, out var result)
                , result))
            .FirstOrDefault(tup => tup.found)
            .result;
        return idk;
    }

    static IAccessorySource GetHairSource(SourceDescriptor descriptor)
    {
        if (descriptor.Name == Constants.HairDyeSourceName)
        {
            if (hairDyes is null) { Log.Error("Requested a hairdye before the dyes were loaded!"); }

            return hairDyes;
        }
        Hairstyles.TryGetValue(descriptor, out var source); 
        return source;
    }

    static IAccessorySource GetWorldSource(SourceDescriptor descriptor)
    {
        Log.Warning("GetWorldSource() not implemented");
        return null;
    }

    static IAccessorySource GetResourcesSource(SourceDescriptor descriptor)
    {
        Log.Warning($"GetResourcesSource() not implemented");
        return null;
    }

    public static IInstantiable GetInstantiable(AccessoryDescriptor descriptor)
    {
        return GetAccessorySource(descriptor.Source)?.GetInstantiable(descriptor);
    }

    public static MaterialDescriptor GetMaterial(MaterialDescriptor descriptor)
    {
        return GetAccessorySource(descriptor.Source)?.GetMaterial(descriptor);
    }

    public static void NotifyHairReady(List<StoredHair> hair, List<HairDye> dye)
    {
        Log.Debug($"NotifyHairReady({hair.Count()} hairstyles, {dye.Count()} dyes");
        hair.Select(x => (Key: x.Source, Value: x))
            .ForEach(tup => Log.Debug($"Adding {tup.Key}, {tup.Value} to StoredHair dict"))
            .ForEach(x => Hairstyles.TryAdd(x.Key, x.Value));
        hairDyes = new HairDyeSource(dye);
        OnHairLoaded?.Invoke((hair, dye));
    }

    public void Dispose() { if (liveFolder.gameObject) GameObject.Destroy(liveFolder.gameObject); }
}
