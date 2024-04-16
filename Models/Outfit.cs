using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CarolCustomizer.Utils;
using CarolCustomizer.Behaviors;
using BepInEx.Logging;
using CarolCustomizer.Hooks.Watchdogs;
using System.Xml.Linq;

namespace CarolCustomizer.Models;

public class Outfit : IDisposable, IComparable<Outfit>
{
    #region Dependencies
    public Transform storedAsset { get; protected set; }
    #endregion

    #region Public Interface
    virtual public string AssetName => storedAsset.name;
    virtual public string DisplayName { get; private set; }
    virtual public Sprite Sprite => null;
    virtual public string Author => "Crimson Tales";
    public List<StoredAccessory> Accessories => AccDict.Values.ToList();
    private Dictionary<AccessoryDescriptor, StoredAccessory> AccDict = new();
    virtual public HashSet<MaterialDescriptor> MaterialDescriptors { get; private set; } = new();
    
    PelvisWatchdog prefabWatchdog;
    public MeshData meshData => prefabWatchdog.MeshData;
    public BoneData boneData => prefabWatchdog.BoneData;
    public CompData compData => prefabWatchdog.CompData;

    public int CompareTo(Outfit other)
    {
        int displayNameComparison = this.DisplayName.CompareTo(other.DisplayName);
        if (displayNameComparison != 0) return displayNameComparison;
        return this.AssetName.CompareTo(other.AssetName);
    }

    public StoredAccessory GetAccessory(AccessoryDescriptor descriptor)
    {
        AccDict.TryGetValue(descriptor, out StoredAccessory result); 
        result ??=  GetAccessory(descriptor.Name);
        return result;
    }

    public StoredAccessory GetAccessory(string name)
    {
        return AccDict.Values.FirstOrDefault(x => x.Name == name);
    }

    #endregion

    #region Lifecycle
    public Outfit(Transform storedAsset)
    {
        if (!storedAsset) { Log.Error("Outfit constructor was passed a null transform."); return; }
        Log.Debug($"Constructing {storedAsset.name}");
        this.storedAsset = storedAsset;
        this.DisplayName = LocalizationIndex.index.GetLine(this.storedAsset.gameObject.name); 

        var pelvis = storedAsset.RecursiveFindTransform(x => x.name == "CarolPelvis");
        if (!pelvis) { Log.Error("failed to find pelvis during Outfit construction."); return; }

        var earlyWatchdog = pelvis.gameObject.GetComponent<PelvisWatchdog>();
        if (earlyWatchdog) 
        {
            GameObject.DestroyImmediate(earlyWatchdog);
            Log.Debug("Early watchdog found and removed");
        }

        this.prefabWatchdog = pelvis.gameObject.AddComponent<PelvisWatchdog>();
        if (!prefabWatchdog) { Log.Error($"failed to instantiate {DisplayName}'s pelvis watchdog"); return; }
        prefabWatchdog.Awake();

        Log.Debug($"Setting up accessories: {storedAsset.name}.");
        if (!prefabWatchdog.MeshData) Log.Warning("Failed to instantiate meshdata in time.");
        var smrs = prefabWatchdog?.MeshData?.baseMeshes;
        if (smrs is null) { Log.Error("no smrs found in watchdog mesh data.") ; return; }
        foreach (var smr in smrs)
        {
            var newAcc = new StoredAccessory(this, smr);
            AccDict[newAcc] = newAcc;

            foreach (var newMat in newAcc.Materials) { if (newMat is not null) MaterialDescriptors.Add(newMat); }
        }
        Log.Debug($"{this.DisplayName} Outfit constructed.");
    }

    public void Dispose() {
        if (!prefabWatchdog) return;
        if (prefabWatchdog.MeshData) GameObject.DestroyImmediate(prefabWatchdog.MeshData);
        if (prefabWatchdog.BoneData) GameObject.DestroyImmediate(prefabWatchdog.BoneData);
        if (prefabWatchdog.CompData) GameObject.DestroyImmediate(prefabWatchdog.CompData);
        GameObject.DestroyImmediate(prefabWatchdog); 
    }
    #endregion
}
