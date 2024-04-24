using CarolCustomizer.Assets;
using CarolCustomizer.Events;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarolCustomizer.Behaviors.Carol;
/// <summary>
/// Responsible for the clothing of one Carol Instance. 
/// </summary>
public class OutfitManager : IDisposable
{
    #region Dependencies
    SkeletonManager skeletonManager;
    CarolInstance playerManager;
    #endregion

    #region State Management
    Dictionary<StoredAccessory, LiveAccessory> instantiatedAccessories = new();
    Outfit animatorSource;
    #endregion

    #region Public Interface
    public PelvisWatchdog pelvis { get; private set; }
    public Action<AccessoryChangedEvent> AccessoryChanged;
    public string AnimatorSource => 
        animatorSource is not null ? 
        animatorSource.AssetName: 
        Constants.Pyjamas;
    public IEnumerable<StoredAccessory> ActiveAccessories =>
            instantiatedAccessories
            .Where(x => x.Value.isActive)
            .Select(x => x.Key);

    public OutfitManager(CarolInstance player, SkeletonManager skeletonManager, OutfitAssetManager dynamicAssetManager)
    {
        this.skeletonManager = skeletonManager;
        playerManager = player;

        playerManager.SpawnEvent += RefreshSMRs;
        playerManager.SpawnEvent += OnSpawn;
        AccessoryChanged += DebugAccChanged;
        OutfitAssetManager.OnOutfitUnloaded += OnOutfitUnloaded;
    }

    private void DebugAccChanged(AccessoryChangedEvent e) => Log.Debug(e.ToString());

    public void Dispose()
    {
        Log.Debug("OutfitManager.Dispose");

        playerManager.SpawnEvent -= RefreshSMRs;
        playerManager.SpawnEvent -= OnSpawn;
        OutfitAssetManager.OnOutfitUnloaded -= OnOutfitUnloaded;

        SetBaseVisibility(true);//Ensure the player is visible when we leave.
        foreach (var liveAcc in instantiatedAccessories.Values) { liveAcc?.Dispose(); } 
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

    private void OnSpawn(PelvisWatchdog pelvis)
    {
        this.pelvis = pelvis;
        HideBase();
        if (animatorSource is null) return;
        SetAnimator(animatorSource);
    }

    public void HideBase() => SetBaseVisibility(false);

    public void SetBaseVisibility(bool visible) => pelvis?.SetBaseVisibility(visible);

    public void SetAnimator(Outfit outfit)
    {
        if (!pelvis) { Log.Warning("Tried to swap animators with no pelviswatchdog instantiated."); return; }
        if (outfit is null) { Log.Warning("Tried to load animator from null outfit"); return; }
        Log.Debug($"changing animator to {outfit.DisplayName}");
        pelvis.SetAnimator(outfit);
        this.animatorSource = outfit;
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