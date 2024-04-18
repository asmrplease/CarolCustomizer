using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CarolCustomizer.Assets;
using CarolCustomizer.Models;
using CarolCustomizer.Utils;
using CarolCustomizer.Events;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Models.Accessories;

namespace CarolCustomizer.Behaviors;
/// <summary>
/// Responsible for the clothing of one Carol Instance. 
/// </summary>
public class OutfitManager : IDisposable
{
    #region Dependencies
    private SkeletonManager skeletonManager;
    private CarolInstance playerManager;
    public PelvisWatchdog pelvis { get; private set; }
    Outfit BaseOutfit;
    #endregion

    #region State Management
    private Dictionary<StoredAccessory, LiveAccessory> instantiatedAccessories = new();
    #endregion

    #region Public Interface
    public string BaseOutfitName => BaseOutfit is not null? BaseOutfit.AssetName : Constants.Pyjamas;
    public bool BaseVisible { get; private set; }
    public int BaseAccessorySlot => 0;
    public Action<AccessoryChangedEvent> AccessoryChanged;
    public IEnumerable<StoredAccessory> ActiveAccessories =>
            instantiatedAccessories
            .Where(x => x.Value.isActive)
            .Select(x => x.Key);

    public OutfitManager(CarolInstance player, SkeletonManager skeletonManager, OutfitAssetManager dynamicAssetManager)
    {
        this.skeletonManager = skeletonManager;
        this.playerManager = player;
        this.BaseVisible = true;

        this.playerManager.SpawnEvent += RefreshSMRs;
        this.playerManager.SpawnEvent += OnSpawn;
        this.AccessoryChanged += DebugAccChanged;
        OutfitAssetManager.OnOutfitUnloaded += OnOutfitUnloaded;
    }

    private void DebugAccChanged(AccessoryChangedEvent e) => Log.Debug(e.ToString());

    public void Dispose()
    {
        Log.Debug("OutfitManager.Dispose");
        SetBaseVisibility(true);//Ensure the player is visible when we leave.
        foreach (var liveAcc in instantiatedAccessories.Values) { liveAcc.Dispose(); }

        this.playerManager.SpawnEvent -= RefreshSMRs;
        this.playerManager.SpawnEvent -= OnSpawn;
        OutfitAssetManager.OnOutfitUnloaded -= OnOutfitUnloaded;
    }

    private void RefreshSMRs(PelvisWatchdog pelvis)
    {
        foreach (var accessory in ActiveAccessories) { instantiatedAccessories[accessory].Refresh(); }
    }

    public void EnableAccessory(StoredAccessory accessory)
    {
        if (!instantiatedAccessories.ContainsKey(accessory)) { Instantiate(accessory); }
        instantiatedAccessories[accessory].Enable();
        var liveAccessory = instantiatedAccessories[accessory] as AccessoryDescriptor;
        AccessoryChanged?.Invoke(new AccessoryChangedEvent(accessory, liveAccessory, true));
    }

    public bool IsEnabled(StoredAccessory accessory)
    {
        if (!instantiatedAccessories.ContainsKey(accessory)) { return false; }
        return instantiatedAccessories[accessory].isActive;
    }

    public void DisableAccessory(StoredAccessory accessory)
    {
        if (!instantiatedAccessories.ContainsKey(accessory)) { Log.Warning("Tried to disable an accessory that was never instantiated."); return; }
        instantiatedAccessories[accessory].Disable();
        var liveAccessory = instantiatedAccessories[accessory] as AccessoryDescriptor;
        AccessoryChanged?.Invoke(new AccessoryChangedEvent(accessory, liveAccessory, false));
    }

    public void DisableAllAccessories()
    {
        //TODO: only call this on active accessories
        foreach (var accessory in instantiatedAccessories.Keys) { DisableAccessory(accessory); }
    }

    public void PaintAccessory(StoredAccessory accessory, MaterialDescriptor material, int index)
    {
        EnableAccessory(accessory);
        instantiatedAccessories[accessory].ApplyMaterial(material, index);
        var liveAccessory = instantiatedAccessories[accessory] as AccessoryDescriptor;
        AccessoryChanged?.Invoke(new AccessoryChangedEvent(accessory, liveAccessory, true));
    }

    public MaterialDescriptor[] GetLiveMaterials(StoredAccessory accessory)
    {
        if (!instantiatedAccessories.ContainsKey(accessory)) { return null; }
        return instantiatedAccessories[accessory].Materials;
    }
    #endregion

    private void Instantiate(StoredAccessory accessory)
    {
        var liveAcc = accessory.BringLive(skeletonManager, OutfitAssetManager.liveFolder);
        instantiatedAccessories.Add(accessory, liveAcc);
        liveAcc.Enable();
        liveAcc.Refresh();
    }

    public void ToggleBaseVisibility()
    {
        this.BaseVisible = false;//!this.BaseVisible;
        RefreshBaseVisibility();
    }

    private void OnSpawn(PelvisWatchdog pelvis)
    {
        this.pelvis = pelvis;
        SetBaseVisibility(false);
        RefreshBaseVisibility();
    }

    public void RefreshBaseVisibility() => SetBaseVisibility(this.BaseVisible); //TODO: better approach

    public void SetBaseVisibility(bool visible) => this.pelvis?.SetBaseVisibility(visible);

    public void SetBaseOutfit(Outfit outfit)
    {
        if (!this.pelvis) { Log.Warning("Tried to swap outfits with no pelviswatchdog instantiated."); return; }
        this.pelvis.SetBaseOutfit(outfit);
        this.BaseOutfit = outfit;
    }

    private void OnOutfitUnloaded(Outfit outfit)
    {
        //for each accessory in the outfit, find any live accessories and remove them from the dict
        foreach (var storedAcc in outfit.Accessories)
        {
            if (!instantiatedAccessories.ContainsKey(storedAcc)) continue;
            var liveAcc = instantiatedAccessories[storedAcc];
            if (liveAcc is not null) { liveAcc.Dispose(); }
            instantiatedAccessories.Remove(storedAcc); 
        }
    }
}