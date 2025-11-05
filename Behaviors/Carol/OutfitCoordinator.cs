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
    readonly HairstyleManager hairstyleManager;
    readonly EffectManager effectManager;
    readonly FaceCopier faceCopier;
    #endregion

    #region Public Interface
    public PelvisWatchdog pelvis { get; private set; }
    public event Action<AccessoryChangedEvent> AccessoryChanged;
    public event Action<HairChangeEvent> HairstyleChanged;

    public string AnimatorSource => effectManager.AnimatorSource;
    public string ConfigurationSource => effectManager.ConfigurationSource;
    public string ColliderSource => magicaManager.ColliderSourceName;
    public IEnumerable<StoredAccessory> ActiveAccessories => accessoryManager.ActiveAccessories;
    public IEnumerable<AccessoryDescriptor> LiveAccessoryDescriptors => accessoryManager.LiveAccessoryDescriptors;
    public IEnumerable<string> ActiveEffects => effectManager.ActiveEffects;
    public string HairstyleName => hairstyleManager.HairstyleName;
    public string HairMaterialName => hairstyleManager.HairMaterialName;
    public void SetHairstyle(StoredHair style) =>  hairstyleManager.AssignHairstyle(style.hairstyle);
    public void SetHairColor(Material hairMat, bool dissolve = false) => hairstyleManager.AssignMaterial(hairMat, dissolve);
    public void SetEffect(Outfit source, bool enabled) => effectManager.SetEffect(source, enabled);
    public void SetAnimator(Outfit source) => effectManager.SetAnimator(source);
    public void PaintAccessory(StoredAccessory accessory, MaterialDescriptor material, int index) => accessoryManager.PaintAccessory(accessory, material, index);
    public void PaintAccessoryShared(StoredAccessory accessory, List<Material> materials) => accessoryManager.PaintAccessoryShared(accessory, materials);
    public void DisableAllAccessories() => accessoryManager.DisableAllAccessories();
    public void DisableAllEffects() => effectManager.DisableAllEffects();
    public void SetConfiguration(HaDSOutfit source) => effectManager.SetConfiguration(source);
    public void SetColliderSource(Outfit source) => magicaManager.SetColliderSource(source);
    public void EnableAccessory(StoredAccessory accessory) => accessoryManager.EnableAccessory(accessory);
    public void DisableAccessory(StoredAccessory accessory) => accessoryManager.DisableAccessory(accessory);
    public MaterialDescriptor[] GetLiveMaterials(StoredAccessory accessory) => accessoryManager?.GetLiveMaterials(accessory);
    public void HandleNewPelvis(PelvisWatchdog watchdog) => this.pelvis = watchdog;


    public OutfitCoordinator(CarolInstance carol, GameObject parent)
    {
        this.skeletonManager  = new SkeletonManager();
        this.faceCopier       = parent.AddComponent<FaceCopier>().Constructor(carol);
        this.hairstyleManager = new HairstyleManager();
        this.accessoryManager = new AccessoryManager(skeletonManager, faceCopier);
        this.magicaManager    = new MagicaManager(skeletonManager);
        this.effectManager    = new EffectManager(skeletonManager);

        carol.SpawnEvent += this.HandleNewPelvis;
        carol.SpawnEvent += this.magicaManager.HandleNewPelvis;
        carol.SpawnEvent += this.skeletonManager.HandleNewPelvis;
        carol.SpawnEvent += this.accessoryManager.HandleNewPelvis;
        carol.SpawnEvent += this.effectManager.HandleNewPelvis;
        carol.SpawnEvent += this.hairstyleManager.HandleNewPelvis;
        carol.SpawnEvent += this.faceCopier.HandleNewPelvis;

        accessoryManager.AccessorySourceAdded += magicaManager.HandleSourceSetup;

        this.hairstyleManager.HairstyleChanged += OnHairChanged;
        this.accessoryManager.AccessoryChanged += OnAccessoryChanged;
    }

    public void Dispose()
    {
        accessoryManager.Dispose();
        hairstyleManager.Dispose();
        GameObject.Destroy(faceCopier);
    }
    #endregion

    void OnAccessoryChanged(AccessoryChangedEvent e) => AccessoryChanged?.Invoke(e);
    void OnHairChanged(HairChangeEvent e) => HairstyleChanged?.Invoke(e);

}