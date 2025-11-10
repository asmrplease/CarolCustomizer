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

namespace CarolCustomizer.Behaviors.Carol;

public class AccessoryManager : IDisposable, IPelvisFollower
{
    Dictionary<AccessoryDescriptor, LiveAccessory> liveAccessories = [];
    PelvisWatchdog pelvis;
    SkeletonManager skeletonManager;
    FaceCopier faceCopier;
    HashSet<SourceDescriptor> InitializedSources = [];

    public event Action<AccessoryChangedEvent> AccessoryChanged;
    public event Action<IAccessorySource> AccessorySourceAdded;

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
        AccessoryChanged?.Invoke(new AccessoryChangedEvent(desc, live, true));
    }

    void CheckExistingSources(AccessoryDescriptor acc)
    {
        if (InitializedSources.Contains(acc.Source)) { return; }
       
        var source = OutfitAssetManager.GetAccessorySource(acc.Source);
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

    public void PaintAccessory(AccessoryDescriptor target, MaterialDescriptor material, int index)
    {
        if (!liveAccessories.TryGetValue(target, out var live)) { Log.Warning($"PaintAccessory failed to find {target}."); return; }

        live.ApplyMaterial(material, index);
        var newState = liveAccessories[target] as AccessoryDescriptor;
        AccessoryChanged?.Invoke(new AccessoryChangedEvent(target, newState, true));
    }

    public void PaintAccessoryShared(AccessoryDescriptor accessory, List<Material> materials)
    {
        EnableAccessory(accessory);
        if (!liveAccessories.ContainsKey(accessory)) { Log.Warning("asked to paint disabled accessory"); return; }
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

    void OnSourceUnloaded(IAccessorySource source)
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
