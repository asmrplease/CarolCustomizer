using CarolCustomizer.Assets;
using CarolCustomizer.Contracts;
using CarolCustomizer.Events;
using CarolCustomizer.Hooks;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CarolController;

namespace CarolCustomizer.Behaviors.Carol;

public class AccessoryManager : IDisposable, IPelvisFollower
{
    Dictionary<AccessoryDescriptor, LiveAccessory> liveAccessories = [];
    PelvisWatchdog pelvis;
    SkeletonManager skeletonManager;
    FaceCopier faceCopier;
    HashSet<SourceDescriptor> InitializedSources = [];

    public event Action<AccessoryChangedEvent> AccessoryChanged;
    public event Action<IGenericSource> AccessorySourceAdded;

    public IEnumerable<AccessoryDescriptor> ActiveAccessories =>
        liveAccessories
        .Where(x => x.Value.isActive)
        .Select(x => x.Key);

    public IEnumerable<AccessoryDescriptor> LiveAccessoryDescriptors =>
        liveAccessories
        .Values
        .Where(x => x.isActive)
        .Select(x => new AccessoryDescriptor(x));

    public AccessoryManager(SkeletonManager skeletonManager, FaceCopier faceCopier)
    {
        this.skeletonManager = skeletonManager;
        this.faceCopier = faceCopier;
        OutfitAssetManager.OnOutfitUnloaded += OnSourceUnloaded;
    }

    public void EnableAccessory(AccessoryDescriptor desc)
    {
        Log.Debug($"EnableAccessory({desc.Name})");
        if (!pelvis) { Log.Warning("No pelvis during EnableAccessory"); return; }

        if (!liveAccessories.TryGetValue(desc, out var live))
        {
            var target = OutfitAssetManager.GetInstantiable(desc);
            if (target is null) { Log.Warning("failed to find accessory"); return; }

            live = target.MakeLive(skeletonManager, faceCopier, OutfitAssetManager.liveFolder);
            CheckExistingSources(desc);
            liveAccessories.Add(desc, live);
        }
        live.Enable();
        if (!live.IsOnArmature(pelvis.transform)) skeletonManager.AssignLiveBones(live);
        //desc.Materials
        //    .Select((mat, index) => (mat, index))
        //    .Where(tup => tup.mat.referenceMaterial)
        //    .ForEach(tup => PaintAccessory(desc, tup.mat, tup.index));
        var e = new AccessoryChangedEvent(desc, live, true);
        AccessoryChanged?.Invoke(e);
        Log.Debug(e.ToString());
    }

    void CheckExistingSources(AccessoryDescriptor acc)
    {
        if (InitializedSources.Contains(acc.Source)) { return; }
       
        var source = OutfitAssetManager.GetSource(acc.Source);
        if (source is null) { Log.Warning($"Failed to find {source}"); return; }

        InitializedSources.Add(acc.Source);
        Log.Debug("Notifying Source Setup");
        AccessorySourceAdded?.Invoke(source);
    }

    public void DisableAccessory(AccessoryDescriptor target)
    {
        if (!liveAccessories.ContainsKey(target)) { Log.Warning($"Tried to disable accessory {target} that was never instantiated."); return; }

        liveAccessories[target].Disable();
        var liveAccessory = liveAccessories[target] as AccessoryDescriptor;
        AccessoryChanged?.Invoke(new AccessoryChangedEvent(target, liveAccessory, false));
    }

    public void PaintAtIndex(AccessoryDescriptor target, MaterialDescriptor material, int index)
    {
        if (!liveAccessories.TryGetValue(target, out var live)) { Log.Warning($"PaintAtIndex failed to find {target}."); return; }

        live.ApplyMaterial(material, index);
        var newState = liveAccessories[target] as AccessoryDescriptor;
        AccessoryChanged?.Invoke(new AccessoryChangedEvent(target, newState, true));
    }

    public void PaintArray(AccessoryDescriptor target, MaterialDescriptor[] materials)
    {
        if (!liveAccessories.TryGetValue(target, out var live)) { Log.Warning($"PaintByList failed to find {target}."); return; }

        live.Materials = materials;
        var arr = materials.Select(x => x.referenceMaterial).ToArray();
        live.ApplyMaterialArray(arr);
        var newState = live as AccessoryDescriptor;
        AccessoryChanged?.Invoke(new AccessoryChangedEvent(target, newState, true));   
    }

    public void PaintShared(AccessoryDescriptor accessory, List<Material> materials)
    {
        EnableAccessory(accessory);
        if (!liveAccessories.ContainsKey(accessory)) { Log.Warning("PaintShared asked to paint disabled accessory"); return; }
        liveAccessories[accessory].ApplySharedMaterials(materials);
    }

    public MaterialDescriptor[] GetLiveMaterials(AccessoryDescriptor accessory)
    {
        if (!liveAccessories.ContainsKey(accessory)) { return null; }
        return liveAccessories[accessory].Materials;
    }

    public void DisableAllAccessories() => ActiveAccessories.ForEach(DisableAccessory);

    void RefreshSMRs(PelvisWatchdog pelvis)
    {
        Log.Debug("RefreshSMRS()");
        liveAccessories.Keys
            .ForEach(CheckExistingSources);
        liveAccessories
            .Values
            .Where(x => x.isActive)
            .ForEach(x => skeletonManager.AssignLiveBones(x, true));
        
    }

    void OnPelvisVisibleChanged(bool visible) =>
        liveAccessories.Values
            .Where(x => x.isActive)
            .ForEach(x => x.SetVisible(visible));

    void OnSourceUnloaded(IGenericSource source)
    {
        foreach (var storedAcc in source.GetAccessories())
        {
            liveAccessories.TryGetValue(storedAcc, out var liveAcc);
            if (liveAcc is null) continue;

            liveAcc.Dispose();
            liveAccessories.Remove(storedAcc);
        }
    }

    public void Dispose()
    {
        liveAccessories.Values
            .ToList()
            .ForEach(x => x.Dispose());
    }

    public void HandleNewPelvis(PelvisWatchdog watchdog)
    {
        if (this.pelvis) this.pelvis.VisibilityChanged -= OnPelvisVisibleChanged;
        this.pelvis = watchdog;
        InitializedSources.Clear();
        this.pelvis.VisibilityChanged += OnPelvisVisibleChanged;
        OnPelvisVisibleChanged(pelvis.Visible);
        RefreshSMRs(pelvis);
    }
}
