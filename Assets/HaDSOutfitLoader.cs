using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using Onirism.Gameplay;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace CarolCustomizer.Assets;
internal class HaDSOutfitLoader : IDisposable
{
    static string ListName = "HaDS Outfits";
    static Dictionary<SourceDescriptor, HaDSOutfit> HaDSOutfits = new();
    static List<Accessory> Accessories = [];
    static List<Accessory> SMRAccessory => Accessories.Where(x => x.mesh is SkinnedMeshRenderer).ToList();

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

        var sorted = vanillaOutfits
            .Select(x => (md: x, name: x.gameObject.name.Split('_')[1]))
            .OrderBy(tup => PrioritySources.IsLowPriority(tup.name))
            .ForEach(tup => Log.Debug(tup.name))
            .Select(tup => tup.md);

        foreach (var outfit in sorted)
        {
            LoadHaDSOutfit(outfit);
            yield return null;
        }

        OutfitAssetManager.OnOutfitSetLoaded?.Invoke();
        //CCPlugin.uiInstance.loadingIndicator.NotifyLoadingComplete(reference);
        LoadAccessories();
    }

    void LoadHair()
    {
        var table = LocalizationSettings.StringDatabase.GetTable((TableReference)"Main", LocalizationSettings.SelectedLocale);
        table.AddEntry("Haircut_Itsuki", "Itsuki");
        table.AddEntry("Haircut_Powerhelmet", "Hood/Helmet");
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

    void LoadAccessories()
    {
        Resources
            .FindObjectsOfTypeAll<Accessory>()
            //.Where(x => x.mesh is SkinnedMeshRenderer)
            //.Select(x => new StoredAccessory())
            .ForEach(Accessories.Add);
    }

    public void Dispose()
    {
        HaDSOutfits.Values.ForEach(x => x.Dispose());
        //foreach (var outfit in HaDSOutfits.Values) { outfit.Dispose(); }
    }

}
