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
    Dictionary<StoredAccessory, LiveAccessory> liveAccessories = [];
    PelvisWatchdog pelvis;
    SkeletonManager skeletonManager;
    FaceCopier faceCopier;

    public event Action<AccessoryChangedEvent> AccessoryChanged;
    public event Action<IAccessorySource> AccessorySourceAdded;

    public IEnumerable<StoredAccessory> ActiveAccessories =>
        liveAccessories
        .Where(x => x.Value.isActive)
        .Select(x => x.Key);

    public IEnumerable<AccessoryDescriptor> LiveAccessoryDescriptors =>
        liveAccessories
        .Values
        .Where(x => x.isActive)
        .Select(x => new AccessoryDescriptor(x));
    public IEnumerable<Outfit> ActiveOutfits =>
        ActiveAccessories
        .Select(x => x.outfit)
        .Distinct();

    public AccessoryManager(SkeletonManager skeletonManager, FaceCopier faceCopier)
    {
        this.skeletonManager = skeletonManager;
        this.faceCopier = faceCopier;
        OutfitAssetManager.OnOutfitUnloaded += OnSourceUnloaded;
    }

    public void EnableAccessory(StoredAccessory accessory)
    {
        Log.Debug($"EnableAccessory({accessory.Name})");
        if (!pelvis) { Log.Warning("No pelvis during EnableAccessory"); return; }

        if (!liveAccessories.TryGetValue(accessory, out var live))
        {
            live = accessory.MakeLive(skeletonManager, faceCopier, OutfitAssetManager.liveFolder);
            CheckExistingSources(accessory);
            liveAccessories.Add(accessory, live);
        }
        live.Enable();
        if (!live.IsOnArmature(pelvis.transform)) skeletonManager.AssignLiveBones(live);
        AccessoryChanged?.Invoke(new AccessoryChangedEvent(accessory, live, true));
    }

    void CheckExistingSources(StoredAccessory acc)
    {
        bool exists = liveAccessories
            .Select(kvp => kvp.Key)
            .Where(existing => existing.Source == acc.Source)
            .Any();
        if (exists) { return; }

        var source = OutfitAssetManager.GetAccessorySource(acc.Source);
        AccessorySourceAdded?.Invoke(source);
    }

    public void DisableAccessory(StoredAccessory accessory)
    {
        if (!liveAccessories.ContainsKey(accessory)) { Log.Warning($"Tried to disable accessory {accessory} that was never instantiated."); return; }

        liveAccessories[accessory].Disable();
        var liveAccessory = liveAccessories[accessory] as AccessoryDescriptor;
        AccessoryChanged?.Invoke(new AccessoryChangedEvent(accessory, liveAccessory, false));
    }

    public void PaintAccessory(StoredAccessory accessory, MaterialDescriptor material, int index)
    {
        liveAccessories[accessory].ApplyMaterial(material, index);
        var liveAccessory = liveAccessories[accessory] as AccessoryDescriptor;
        AccessoryChanged?.Invoke(new AccessoryChangedEvent(accessory, liveAccessory, true));
    }

    public void PaintAccessoryShared(StoredAccessory accessory, List<Material> materials)
    {
        EnableAccessory(accessory);
        if (!liveAccessories.ContainsKey(accessory)) { Log.Warning("asked to paint disabled accessory"); return; }
        liveAccessories[accessory].ApplySharedMaterials(materials);
    }

    public MaterialDescriptor[] GetLiveMaterials(StoredAccessory accessory)
    {
        if (!liveAccessories.ContainsKey(accessory)) { return null; }
        return liveAccessories[accessory].Materials;
    }

    public void DisableAllAccessories() => ActiveAccessories.ForEach(DisableAccessory);

    void RefreshSMRs(PelvisWatchdog pelvis)
    {
        Log.Debug("RefreshSMRS()");
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
        this.pelvis.VisibilityChanged += OnPelvisVisibleChanged;
        OnPelvisVisibleChanged(pelvis.Visible);
        RefreshSMRs(pelvis);
    }
}
