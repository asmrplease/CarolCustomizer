using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Models.Outfits;

public class Outfit : IDisposable, IComparable<Outfit>
{
    #region Dependencies
    public Transform storedAsset { get; protected set; }
    #endregion

    #region Public Interface
    public string AssetName { get; protected set; }
    virtual public string DisplayName { get; private set; }
    virtual public Sprite Sprite => null;
    virtual public string Author => "Crimson Tales";

    protected Dictionary<AccessoryDescriptor, StoredAccessory> AccDict = new();

    public List<StoredAccessory> Accessories => AccDict.Values.ToList();
    public HashSet<MaterialDescriptor> MaterialDescriptors { get; private set; } = new();

    public PelvisWatchdog prefabWatchdog { get; private set; }
    public BoneData boneData => prefabWatchdog.BoneData;
    public CompData compData => prefabWatchdog.CompData;

    virtual public Func<SkinnedMeshRenderer, bool> FaceDefinition => (x) => x.name == "tete";

    #endregion

    #region Lifecycle
    public Outfit(Transform storedAsset)
    {
        if (!storedAsset) { Log.Error("Outfit constructor was passed a null transform."); return; }
        Log.Debug($"Constructing {storedAsset.name}");
        this.storedAsset = storedAsset;
        AssetName = storedAsset.name;
        DisplayName = LocalizationIndex.index.GetLine(this.storedAsset.gameObject.name).Replace("CAROL_", "");

        var pelvis = storedAsset.RecursiveFindTransform(x => x.name == "CarolPelvis");
        if (!pelvis) { Log.Error("failed to find pelvis during Outfit construction."); return; }

        var earlyWatchdog = pelvis.gameObject.GetComponent<PelvisWatchdog>();
        if (earlyWatchdog)
        {
            UnityEngine.Object.DestroyImmediate(earlyWatchdog);
            Log.Debug("Early watchdog found and removed");
        }

        prefabWatchdog = pelvis.gameObject.AddComponent<PelvisWatchdog>();
        if (!prefabWatchdog) { Log.Error($"failed to instantiate {DisplayName}'s pelvis watchdog"); return; }
        prefabWatchdog.Awake();

        Log.Debug($"Setting up accessories: {storedAsset.name}.");
        if (!prefabWatchdog.CompData) Log.Warning("Failed to instantiate meshdata in time.");
        var smrs = prefabWatchdog?.CompData?.allSMRs;
        if (smrs is null) { Log.Error("no smrs found in watchdog mesh data."); return; }
        foreach (var smr in smrs)
        {
            var newAcc = new StoredAccessory(this, smr);
            AccDict[newAcc] = newAcc;

            foreach (var newMat in newAcc.Materials) { if (newMat is not null) MaterialDescriptors.Add(newMat); }
        }
        Log.Debug($"{DisplayName} Outfit constructed.");
    }

    public int CompareTo(Outfit other)
    {
        int displayNameComparison = DisplayName.CompareTo(other.DisplayName);
        if (displayNameComparison != 0) return displayNameComparison;
        return AssetName.CompareTo(other.AssetName);
    }

    public StoredAccessory GetAccessory(AccessoryDescriptor descriptor)
    {
        AccDict.TryGetValue(descriptor, out StoredAccessory result);
        result ??= GetAccessory(descriptor.Name);
        return result;
    }

    public StoredAccessory GetAccessory(string name) => AccDict.Values.FirstOrDefault(x => x.Name == name);

    public void Dispose()
    {
        if (!prefabWatchdog) return;
        if (prefabWatchdog.BoneData) UnityEngine.Object.DestroyImmediate(prefabWatchdog.BoneData);
        if (prefabWatchdog.CompData) UnityEngine.Object.DestroyImmediate(prefabWatchdog.CompData);
        UnityEngine.Object.DestroyImmediate(prefabWatchdog);
    }
    #endregion
}
