using CarolCustomizer.Contracts;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Utils;
using MagicaCloth2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Models.Outfits;

public class Outfit : IDisposable, IComparable<Outfit>, IEquatable<Outfit>, IGenericSource
{
    #region Dependencies
    public Transform storedAsset { get; protected set; }
    #endregion

    #region Public Interface
    public string AssetName { get; protected set; }
    virtual public string DisplayName { get; private set; }
    virtual public Sprite Sprite => null;
    virtual public string Author => "Crimson Tales";

    public SourceDescriptor Descriptor { get; private set; }

    virtual public ModelData GetConfiguration() => null;

    protected Dictionary<AccessoryDescriptor, StoredAccessory> AccDict = [];

    public List<StoredAccessory> GetAccessories() => AccDict.Values.ToList();
    public List<MaterialDescriptor> GetMaterials() => MaterialDescriptors.ToList();
    List<OutfitEffect> Effects = [];
    public HashSet<MaterialDescriptor> MaterialDescriptors { get; private set; } = [];

    public PelvisWatchdog prefabWatchdog { get; private set; }
    public BoneData boneData => prefabWatchdog.BoneData;
    public CompData compData => prefabWatchdog.CompData;
    public MagiData magiData => prefabWatchdog.MagiData;
    virtual public Func<SkinnedMeshRenderer, bool> FaceDefinition => (x) => x.name == "tete";

    public MaterialDescriptor GetMaterial(MaterialDescriptor descriptor)
    {
        MaterialDescriptors.TryGetValue(descriptor, out var result);
        return result;
    }

    public List<Transform> GetBespokeBones()
    {
        return boneData.BespokeBones;
    }

    public List<MagicaCloth> GetBoneCloths() => magiData.BoneCloths;

    #endregion

    #region Lifecycle
    public Outfit(Transform storedAsset)
    {
        if (!storedAsset) { Log.Error("Outfit constructor was passed a null transform."); return; }

        this.storedAsset = storedAsset;
        AssetName = storedAsset.name;
        var localization = LocalizationIndex.GetLine(this.storedAsset.gameObject.name);
        if (localization.StartsWith("NOKEY")) localization = AssetName.Replace("CAROL_", "").Trim();
        if (localization == "") localization = "Bombsuit";
        DisplayName = localization;
        Descriptor = new SourceDescriptor(AssetName, SourceType.Outfit);
        var pelvis = storedAsset.RecursiveFindTransform(x => x.name == Constants.Pelvis);
        if (!pelvis) { Log.Error("failed to find pelvis during Outfit construction."); return; }

        var duplicates = pelvis
            .GetComponentsInChildren<Transform>(true)
            .GroupBy(x => x.name)
            .Where(x => x.Count() > 1);
        foreach (var grouping in duplicates)
        {
            int i = 0;
            grouping.ForEach(x => x.name += i++);
        }

        prefabWatchdog = PelvisWatchdog.GetAddWatchdog(pelvis.gameObject);
        prefabWatchdog.Awake();
        if (!prefabWatchdog.CompData) Log.Warning("Failed to instantiate meshdata in time.");
        var smrs = prefabWatchdog?.CompData?.allSMRs;
        if (smrs is null) { Log.Error("no smrs found in watchdog mesh data."); return; }

        smrs
            .Select(smr => new StoredAccessory(this, smr))
            .ForEach(acc => AccDict[acc] = acc)
            .SelectMany(acc => acc.Materials)
            .Where(mat => mat is not null)
            .ForEach(mat => MaterialDescriptors.Add(mat));

        foreach (var effect in compData.EffectBehaviours)
        {
            Effects.Add(
                new OutfitEffect(
                    effect.transform.GetAddressRelativeTo(prefabWatchdog.transform),
                    OutfitEffect.ComponentType.Behavior,
                    this.Descriptor)
                );
        }
        foreach (var effect in compData.EffectGameObjects)
        {
            Effects.Add(
                new OutfitEffect(
                    effect.transform.GetAddressRelativeTo(prefabWatchdog.transform),
                    OutfitEffect.ComponentType.Component,
                    this.Descriptor)
                );
        }

        Log.Debug($"{DisplayName} constructed.");
    }

    public StoredAccessory GetAccessory(AccessoryDescriptor descriptor)
    {
        AccDict.TryGetValue(descriptor, out StoredAccessory result);
        if (result == null)
        {
            Log.Warning($"failed to match {descriptor}, looking up accessory by name.");
            result = GetAccessoryByName(descriptor.Name);
            if (result == null) Log.Warning($"Accessory {descriptor.Name} is not in {descriptor.Source}.");
        }
        return result;
    }

    public StoredAccessory GetAccessoryByName(string name)
    {
        var result = AccDict.Values.FirstOrDefault(x => x.Name == name);
        if (result is null) { Log.Warning($"failed to find {name} by name in {Descriptor}."); }
        //else { Log.Debug($"found {name} in {Descriptor}"); }
        return result;
    }

    public virtual RuntimeAnimatorController GetAnimator() => null;

    public void Dispose()
    {
        Log.Debug("Outfit.Dispose()");
        if (prefabWatchdog) GameObject.Destroy(prefabWatchdog);
    }
    #endregion

    #region Equality & Comparison
    public int CompareTo(Outfit other)
    {
        int displayNameComparison = DisplayName.CompareTo(other.DisplayName);
        if (displayNameComparison != 0) return displayNameComparison;
        return AssetName.CompareTo(other.AssetName);
    }
    public bool Equals(Outfit other)
    {
        return this.AssetName == other.AssetName;
    }

    public override bool Equals(object other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != GetType()) return false;

        return Equals((Outfit)other);
    }

    public static bool operator ==(Outfit lhs, Outfit rhs) => Equals(rhs, lhs);
    public static bool operator !=(Outfit lhs, Outfit rhs) => !Equals(rhs, lhs);

    public override int GetHashCode()
    {
        return this.AssetName.GetHashCode();
    }

    public List<OutfitEffect> GetEffects() => this.Effects;

    List<MagicaCapsuleCollider> IMagicaSource.GetColliders() => this.magiData.CapsuleColliders;

    IInstantiable IModelProvider.GetInstantiable(AccessoryDescriptor accessory) => GetAccessory(accessory);
    #endregion
}
