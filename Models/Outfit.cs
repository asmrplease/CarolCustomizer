﻿using System;
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

    public int CompareTo(Outfit other)
    {
        int displayNameComparison = this.DisplayName.CompareTo(other.DisplayName);
        if (displayNameComparison != 0) return displayNameComparison;
        return this.AssetName.CompareTo(other.AssetName);
    }

    public StoredAccessory GetAccessory(AccessoryDescriptor descriptor)
    {
        AccDict.TryGetValue(descriptor, out StoredAccessory result); 
        result ??= AccDict.Values.FirstOrDefault(x => x.Name == descriptor.Name); ;
        return result;
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

        Log.Debug("Setting up accessories.");
        if (!prefabWatchdog.MeshData) Log.Warning("Failed to instantiate meshdata in time.");
        var smrs = prefabWatchdog.MeshData.baseMeshes;
        if (smrs is null) { Log.Error("no smrs found in watchdog mesh data.") ; return; }
        foreach (var smr in smrs)
        {
            var newAcc = new StoredAccessory(this, smr);
            AccDict[newAcc] = newAcc;

            foreach (var newMat in newAcc.Materials) { if (newMat is not null) MaterialDescriptors.Add(newMat); }
        }
    }

    public void Dispose() { if (prefabWatchdog) GameObject.DestroyImmediate(prefabWatchdog); }
    #endregion
}
