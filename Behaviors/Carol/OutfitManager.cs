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
    Dictionary<StoredAccessory, LiveAccessory> liveAccessories = new();
    Outfit animatorSource;
    HaDSOutfit configurationSource;
    #endregion

    #region Public Interface
    public PelvisWatchdog pelvis { get; private set; }
    public Action<AccessoryChangedEvent> AccessoryChanged;
    public string AnimatorSource => 
        animatorSource is not null?
        animatorSource.AssetName : Constants.Pyjamas;
    public string ConfigurationSource => 
        configurationSource is not null? 
        configurationSource.AssetName : Constants.Pyjamas;

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

    public OutfitManager(CarolInstance player, SkeletonManager skeletonManager, OutfitAssetManager dynamicAssetManager)
    {
        this.skeletonManager = skeletonManager;
        playerManager = player;
        animatorSource = configurationSource = OutfitAssetManager.GetOutfitByAssetName(Constants.Pyjamas);

        playerManager.SpawnEvent += RefreshSMRs;
        playerManager.SpawnEvent += OnSpawn;
        AccessoryChanged += DebugAccChanged;
        OutfitAssetManager.OnOutfitUnloaded += OnOutfitUnloaded;
    }

    void DebugAccChanged(AccessoryChangedEvent e) => Log.Debug(e.ToString());

    public void Dispose()
    {
        Log.Debug("OutfitManager.Dispose");

        playerManager.SpawnEvent -= RefreshSMRs;
        playerManager.SpawnEvent -= OnSpawn;
        OutfitAssetManager.OnOutfitUnloaded -= OnOutfitUnloaded;

        SetBaseVisibility(true);//Ensure the player is visible when we leave. 
    }

    void RefreshSMRs(PelvisWatchdog pelvis)
    {
        foreach (var accessory in ActiveAccessories) { liveAccessories[accessory].Refresh(); }
    }

    public void EnableAccessory(StoredAccessory accessory)
    {
        liveAccessories.TryGetValue(accessory, out var live);
        if (live is null) { live = Instantiate(accessory); }
        else
        {
            live.Enable();
            live.Refresh();
        }
        AccessoryChanged?.Invoke(new AccessoryChangedEvent(accessory, live, true));
    }

    public bool IsEnabled(StoredAccessory accessory)
    {
        if (!liveAccessories.ContainsKey(accessory)) { return false; }
        return liveAccessories[accessory].isActive;
    }

    public void DisableAccessory(StoredAccessory accessory)
    {
        if (!liveAccessories.ContainsKey(accessory)) { Log.Warning("Tried to disable an accessory that was never instantiated."); return; }
        liveAccessories[accessory].Disable();
        var liveAccessory = liveAccessories[accessory] as AccessoryDescriptor;
        AccessoryChanged?.Invoke(new AccessoryChangedEvent(accessory, liveAccessory, false));
    }

    public void DisableAllAccessories()
    {
        //TODO: only call this on active accessories
        foreach (var accessory in liveAccessories.Keys) { DisableAccessory(accessory); }
    }

    public void PaintAccessory(StoredAccessory accessory, MaterialDescriptor material, int index)
    {
        EnableAccessory(accessory);
        liveAccessories[accessory].ApplyMaterial(material, index);
        var liveAccessory = liveAccessories[accessory] as AccessoryDescriptor;
        AccessoryChanged?.Invoke(new AccessoryChangedEvent(accessory, liveAccessory, true));
    }

    public MaterialDescriptor[] GetLiveMaterials(StoredAccessory accessory)
    {
        if (!liveAccessories.ContainsKey(accessory)) { return null; }
        return liveAccessories[accessory].Materials;
    }
    #endregion

    LiveAccessory Instantiate(StoredAccessory accessory)
    {
        var liveAcc = accessory.MakeLive(skeletonManager, OutfitAssetManager.liveFolder);
        liveAccessories.Add(accessory, liveAcc);
        liveAcc.Enable();
        liveAcc.Refresh();
        return liveAcc;
    }

    void OnSpawn(PelvisWatchdog pelvis)
    {
        this.pelvis = pelvis;
        HideBase();
        if (animatorSource is null) return;
        SetAnimator(animatorSource);
        ApplyConfig();
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

    public void SetConfiguration(HaDSOutfit outfit) 
    {
        Log.Debug("SetConfiguration()");
        if (outfit is null) { Log.Warning("outfit is null"); return; }
        this.configurationSource = outfit;
        ApplyConfig();
    }

    void ApplyConfig()
    {
        if (!pelvis) return;
        if (configurationSource is null) return;
        pelvis.SetHeightOffset(configurationSource.modelData.height);
    }

    private void OnOutfitUnloaded(Outfit outfit)
    {
        foreach (var storedAcc in outfit.Accessories)
        {
            if (!liveAccessories.ContainsKey(storedAcc)) continue;
            var liveAcc = liveAccessories[storedAcc];
            if (liveAcc is not null) { liveAcc.DestroyGameObject(); }
            liveAccessories.Remove(storedAcc);
        }
    }
}