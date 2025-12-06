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
    public static HairDyeSource HairDyes;
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

    public static IGenericSource GetSource(SourceDescriptor descriptor, bool warnOnMissing = true)
    {
        var result =  descriptor.Type switch
        {
            SourceType.Outfit    => GetOutfitSource(descriptor),
            SourceType.Hair      => GetHairSource(descriptor),
            SourceType.World     => GetWorldSource(descriptor),
            SourceType.Resources => GetWorldSource(descriptor),
            _ => null,
        };

        if (result is null && warnOnMissing) Log.Warning($"No source of type {descriptor.Type} named {descriptor.Name} was found");
        return result;
    }

    static IGenericSource GetOutfitSource(SourceDescriptor descriptor)
    {
        var idk = outfitSets.Values
            .Select(dict =>
                (found: dict.TryGetValue(descriptor, out var result)
                , result))
            .FirstOrDefault(tup => tup.found)
            .result;
        return idk;
    }

    static IGenericSource GetHairSource(SourceDescriptor descriptor)
    {
        if (descriptor.Name == Constants.HairDyeSourceName)
        {
            if (HairDyes is null) { Log.Error("Requested a hairdye before the dyes were loaded!"); }

            return HairDyes;
        }
        Hairstyles.TryGetValue(descriptor, out var source); 
        return source;
    }

    static IGenericSource GetWorldSource(SourceDescriptor descriptor)
    {
        Log.Warning("GetWorldSource() not implemented");
        return null;
    }

    static IGenericSource GetResourcesSource(SourceDescriptor descriptor)
    {
        Log.Warning($"GetResourcesSource() not implemented");
        return null;
    }

    public static IInstantiable GetInstantiable(AccessoryDescriptor descriptor)
    {
        return GetSource(descriptor.Source)?.GetInstantiable(descriptor);
    }

    public static MaterialDescriptor GetMaterial(MaterialDescriptor descriptor)
    {
        return GetSource(descriptor.Source)?.GetMaterial(descriptor);
    }

    public static void NotifyHairReady(List<StoredHair> hair, List<HairDye> dye)
    {
        Log.Debug($"NotifyHairReady({hair.Count()} hairstyles, {dye.Count()} dyes");
        hair.Select(x => (Key: x.Source, Value: x))
            .ForEach(tup => Log.Debug($"Adding {tup.Key}, {tup.Value} to StoredHair dict"))
            .ForEach(x => Hairstyles.TryAdd(x.Key, x.Value));
        HairDyes = new HairDyeSource(dye);
        OnHairLoaded?.Invoke((hair, dye));
    }

    public static IEnumerable<(string, RecipeDescriptor)> GetAllRecipes()
    {
        return outfitSets.Values
            .SelectMany(sourceDict => sourceDict.Values)
            .SelectMany(outfit => outfit.Variants.Select(kvp => (outfit.DisplayName, kvp.Value)));
    }

    public void Dispose() { if (liveFolder.gameObject) GameObject.Destroy(liveFolder.gameObject); }
}
