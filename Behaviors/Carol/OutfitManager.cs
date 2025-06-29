﻿using CarolCustomizer.Assets;
using CarolCustomizer.Events;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Carol;
/// <summary>
/// Responsible for the clothing of one Carol Instance. 
/// </summary>
public class OutfitManager : IDisposable
{
    #region Dependencies
    readonly SkeletonManager skeletonManager;
    readonly CarolInstance playerManager;
    readonly HairstyleManager hairstyleManager;
    #endregion

    #region State Management
    Dictionary<StoredAccessory, LiveAccessory> liveAccessories = new();
    HashSet<Outfit> outfitEffects = new();
    Outfit animatorSource;
    HaDSOutfit configurationSource;
    Outfit colliderSource;
    #endregion

    #region Public Interface
    public PelvisWatchdog pelvis { get; private set; }
    public event Action<AccessoryChangedEvent> AccessoryChanged;
    public event Action<HairChangeEvent> HairstyleChanged;
    public string AnimatorSource => animatorSource?.AssetName ?? Constants.Pyjamas;
    public string ConfigurationSource => configurationSource?.AssetName ?? Constants.Pyjamas;
    public string ColliderSource => colliderSource?.AssetName ?? Constants.Pyjamas;

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

    public IEnumerable<string> ActiveEffects =>
        outfitEffects
        .Select(x => x.AssetName);

    public string Hairstyle => hairstyleManager.hairstyle ?
        hairstyleManager.hairstyle.name : "";

    public string HairColor => hairstyleManager.hairMaterial ?
        hairstyleManager.hairMaterial.name : "";

    public OutfitManager(CarolInstance player, SkeletonManager skeletonManager)
    {
        this.skeletonManager = skeletonManager;
        this.hairstyleManager = new HairstyleManager(player);
        playerManager = player;
        animatorSource
            = colliderSource
            = configurationSource
            = OutfitAssetManager.GetPyjamas();

        playerManager.SpawnEvent += HandleNewPelvis;
        OutfitAssetManager.OnOutfitUnloaded += OnOutfitUnloaded;
    }

    public void Dispose()
    {
        liveAccessories.Values
            .ToList()
            .ForEach(x => x.Dispose()); 
        hairstyleManager.Dispose();
    }

    void RefreshSMRs(PelvisWatchdog pelvis) 
    {
        Log.Debug("RefreshSMRS()");
        liveAccessories
            .Values
            .Where(x => x.isActive)
            .ForEach(skeletonManager.AssignLiveBones);
    }

    public void EnableAccessory(StoredAccessory accessory)
    {
        Log.Debug($"EnableAccessory({accessory.Name})");
        if (!pelvis) { Log.Warning("No pelvis during EnableAccessory"); return; }

        if (!liveAccessories.TryGetValue(accessory, out var live))
        {
            live = accessory.MakeLive(skeletonManager, OutfitAssetManager.liveFolder);
            liveAccessories.Add(accessory, live);
        }
        live.Enable();
        if (!live.IsOnArmature(pelvis.transform)) skeletonManager.AssignLiveBones(live);
        AccessoryChanged?.Invoke(new AccessoryChangedEvent(accessory, live, true));
    }

    public void DisableAllAccessories() => ActiveAccessories.ForEach(DisableAccessory);

    public void DisableAccessory(StoredAccessory accessory)
    {
        if (!liveAccessories.ContainsKey(accessory)) { Log.Warning("Tried to disable an accessory that was never instantiated."); return; }

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
    #endregion

    void HandleNewPelvis(PelvisWatchdog pelvis)
    {
        Log.Debug("HandleNewPelvis()");
        this.pelvis = pelvis;
        RefreshSMRs(pelvis);
        if (outfitEffects.Any()) RefreshEffects();
        if (animatorSource is not null) SetAnimator(animatorSource);
        if (configurationSource is not null) ApplyConfig();
        if (colliderSource is not null) ApplyCollider();
    }

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

        Log.Debug("ApplyConfig()");
        pelvis.SetHeightOffset(configurationSource.modelData.height);
    }

    public void SetEffect(Outfit outfit, bool enabled)
    {
        if (!pelvis || outfit is null) return;
        if (!outfit.Effects.Any()) return;

        skeletonManager.GetAddBoneSet(outfit);
        foreach (var effect in outfit.Effects)
        {
            var transform = pelvis.transform.Find(effect.RelativePath);
            if (!transform) continue;

            switch (effect.Type)
            {
                case OutfitEffect.ComponentType.Behavior:
                    transform
                        .GetComponents<Behaviour>()
                        .ForEach(x => x.enabled = enabled);
                    break;
                case OutfitEffect.ComponentType.Component:
                    transform
                        .gameObject
                        .SetActive(enabled);
                    break;
            }
        }

        if ( enabled) outfitEffects.Add(outfit);
        if (!enabled) outfitEffects.Remove(outfit);
    }

    public void SetColliderSource(Outfit outfit)
    {
        if (outfit is null) { Log.Warning("outfit was null when setting collider source"); return; }

        colliderSource = outfit;
        ApplyCollider();
    }

    void ApplyCollider()
    {
        if (!pelvis) return;

        var sourceColliders = colliderSource
            .magiData
            .CapsuleColliders
            .Where(x => x)
            .ToDictionary(x=> x.name);
        pelvis.MagiData
            .CapsuleColliders
            .Select(x =>
                (live: x
                ,found: sourceColliders.TryGetValue(x.name, out var reference)
                ,reference))
            .Where(tup => tup.found)
            .ForEach(tup => 
                tup.live.CopyFrom(tup.reference));
    }

    void RefreshEffects()
    {
        outfitEffects
            .ForEach(x =>
               SetEffect(x, true));
    }

    public void DisableAllEffects()
    {
        outfitEffects
            .ToList()
            .ForEach(x=> SetEffect(x, false));
    }

    void OnOutfitUnloaded(Outfit outfit)
    {
        foreach (var storedAcc in outfit.Accessories)
        {
            liveAccessories.TryGetValue(storedAcc, out var liveAcc);
            if (liveAcc is null) continue;

            liveAcc.Dispose();
            liveAccessories.Remove(storedAcc);
        }
    }

    public void SetHairstyle(Hairstyle style) 
    {
        Log.Debug($"Assigning hairstyle {style.name}");
        hairstyleManager.AssignHairstyle(style);
        var e = hairstyleManager.GetHairDescriptor();
        HairstyleChanged?.Invoke(e);

    } 
    public void SetHairColor(Material hairMat) 
    {
        hairstyleManager.AssignMaterial(hairMat);
        var e = hairstyleManager.GetHairDescriptor();
        HairstyleChanged?.Invoke(e);
    } 
    
}