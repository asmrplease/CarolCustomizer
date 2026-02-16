using CarolCustomizer.Contracts;
using CarolCustomizer.Events;
using CarolCustomizer.Hooks;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Models.Outfits;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Carol;
/// <summary>
/// Responsible for the clothing of one Carol Instance. 
/// </summary>
public class OutfitCoordinator : IDisposable, IPelvisFollower
{
    #region Dependencies
    readonly SkeletonManager skeletonManager;
    readonly AccessoryManager accessoryManager;
    readonly MagicaManager magicaManager;
    readonly EffectManager effectManager;
    readonly FaceCopier faceCopier;
    #endregion

    #region Public Interface
    public PelvisWatchdog pelvis { get; private set; }
    public event Action<AccessoryChangedEvent> AccessoryChanged;
    public SourceDescriptor AnimatorSource => effectManager.AnimatorSource;
    public SourceDescriptor ConfigurationSource => effectManager.ConfigurationSource;
    public SourceDescriptor ColliderSource => magicaManager.ColliderSourceName;
    public IEnumerable<AccessoryDescriptor> ActiveAccessories => accessoryManager.ActiveAccessories;
    public IEnumerable<AccessoryDescriptor> LiveAccessoryDescriptors => accessoryManager.LiveAccessoryDescriptors;
    public IEnumerable<SourceDescriptor> ActiveEffects => effectManager.ActiveEffects;
    public void SetEffect(SourceDescriptor source, bool state) => effectManager.SetEffect(source, state);
    public void SetAnimator(SourceDescriptor source) => effectManager.SetAnimator(source);
    public void PaintAtIndex(AccessoryDescriptor accessory, MaterialDescriptor material, int index) => accessoryManager.PaintAtIndex(accessory, material, index);
    public void PaintShared(AccessoryDescriptor accessory, List<Material> materials) => accessoryManager.PaintShared(accessory, materials);
    public void PaintArray(AccessoryDescriptor accessory, MaterialDescriptor[] materials) => accessoryManager.PaintArray(accessory, materials);
    public void DisableAllAccessories() => accessoryManager.DisableAllAccessories();
    public void DisableAllEffects() => effectManager.DisableAllEffects();
    public void SetConfiguration(SourceDescriptor source) => effectManager.SetConfiguration(source);
    public void SetColliderSource(SourceDescriptor source) => magicaManager.SetColliderSource(source);
    public void EnableAccessory(AccessoryDescriptor accessory) => accessoryManager.EnableAccessory(accessory);
    public void DisableAccessory(AccessoryDescriptor accessory) => accessoryManager.DisableAccessory(accessory);
    public MaterialDescriptor[] GetLiveMaterials(AccessoryDescriptor accessory) => accessoryManager?.GetLiveMaterials(accessory);
    public void HandleNewPelvis(PelvisWatchdog watchdog) => this.pelvis = watchdog;


    public OutfitCoordinator(CarolInstance carol, GameObject parent)
    {
        this.skeletonManager  = new SkeletonManager();
        this.faceCopier       = parent.AddComponent<FaceCopier>().Constructor(carol);
        this.accessoryManager = new AccessoryManager(skeletonManager, faceCopier);
        this.magicaManager    = new MagicaManager(skeletonManager);
        this.effectManager    = new EffectManager(skeletonManager);

        carol.SpawnEvent += this.HandleNewPelvis;
        carol.SpawnEvent += this.magicaManager.HandleNewPelvis;
        carol.SpawnEvent += this.skeletonManager.HandleNewPelvis;
        carol.SpawnEvent += this.accessoryManager.HandleNewPelvis;
        carol.SpawnEvent += this.effectManager.HandleNewPelvis;
        carol.SpawnEvent += this.faceCopier.HandleNewPelvis;

        accessoryManager.AccessorySourceAdded += magicaManager.HandleSourceSetup;
        this.accessoryManager.AccessoryChanged += (e) => AccessoryChanged?.Invoke(e);
    }

    public void Dispose()
    {
        accessoryManager.Dispose();
        GameObject.Destroy(faceCopier);
    }
    #endregion

}