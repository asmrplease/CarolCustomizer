using CarolCustomizer.Contracts;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.UI.Outfits;
using CarolCustomizer.Utils;
using MagicaCloth2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace CarolCustomizer.Models.Outfits;

public partial class Outfit 
{
    public Transform storedAsset { get; protected set; }

    public string AssetName { get; protected set; }
    public string DisplayName { get; protected set; }
    public string Author { get; protected set; }

    protected Dictionary<AccessoryDescriptor, StoredAccessory> AccDict = [];
    public HashSet<MaterialDescriptor> MaterialDescriptors { get; private set; } = [];
    List<OutfitEffect> Effects = [];
    public Dictionary<string, Recipe> Variants { get; private set; } = [];

    public PelvisWatchdog prefabWatchdog { get; protected set; }
    public BoneData boneData => prefabWatchdog.BoneData;
    public CompData compData => prefabWatchdog.CompData;
    public MagiData magiData => prefabWatchdog.MagiData;
    virtual public Func<SkinnedMeshRenderer, bool> FaceDefinition => (x) => x.name == "tete";
    public StoredAccessory GetAccessory(string name) => AccDict.Values.FirstOrDefault(x => x.Name == name);

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
        var pelvis = storedAsset.RecursiveFindTransform(x => x.name == "CarolPelvis");
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

    public void Dispose()
    {
        Log.Debug("Outfit.Dispose()");
        if (prefabWatchdog) GameObject.Destroy(prefabWatchdog);
    }
}

public partial class Outfit : IComparable<Outfit>, IEquatable<Outfit>
{
    public int CompareTo(Outfit other)
    {
        int displayNameComparison = DisplayName.CompareTo(other.DisplayName);
        if (displayNameComparison != 0) return displayNameComparison;
        return AssetName.CompareTo(other.AssetName);
    }
    public override bool Equals(object other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != GetType()) return false;

        return Equals((Outfit)other);
    }
    public bool Equals(Outfit other) => this.AssetName == other.AssetName;
    public static bool operator ==(Outfit lhs, Outfit rhs) => Equals(rhs, lhs);
    public static bool operator !=(Outfit lhs, Outfit rhs) => !Equals(rhs, lhs);
    public override int GetHashCode() => this.AssetName.GetHashCode();
}

public partial class Outfit : IGenericSource
{
    public SourceDescriptor Descriptor { get; private set; }
    virtual public ModelData GetConfiguration() => null;
    public List<MagicaCloth> GetBoneCloths() => magiData.BoneCloths;
    public List<StoredAccessory> GetAccessories() => AccDict.Values.ToList();
    public List<Transform> GetBespokeBones() => boneData.BespokeBones;
    public MaterialDescriptor GetMaterial(MaterialDescriptor descriptor)
    {
        MaterialDescriptors.TryGetValue(descriptor, out var result);
        return result;
    }
    public List<MaterialDescriptor> GetMaterials() => MaterialDescriptors.ToList();
    StoredAccessory IModelProvider.GetAccessory(AccessoryDescriptor descriptor)
    {
        AccDict.TryGetValue(descriptor, out StoredAccessory result);
        result ??= GetAccessory(descriptor.Name);
        return result;
    }
    public virtual RuntimeAnimatorController GetAnimator() => null;
    public List<OutfitEffect> GetEffects() => this.Effects;
    List<MagicaCapsuleCollider> IMagicaProvider.GetColliders() => this.magiData.CapsuleColliders;
    IInstantiable IModelProvider.GetInstantiable(AccessoryDescriptor accessory) => GetAccessory(accessory);
}

public partial class Outfit : IListable
{
    public Sprite Thumbnail { get; protected set; }
    string IListable.Header => this.DisplayName;
    string IListable.Subheader => this.Author;
    Color IListable.BaseColor => Constants.DefaultColor;
    Color IListable.HighlightColor => Constants.Highlight;

    IEnumerable<IListable> IListable.Children 
    { 
        get
        {
            List<IListable> results = [];
            this.GetAccessories()
                .Select(x => new MutableModel(x))
                .ForEach(results.Add);
            results.AddRange(this.Variants.Values);
            this.MaterialDescriptors
                .ForEach(results.Add);
            return results;
        }
    }
}

public partial class Outfit : IContextMenuActions
{
    public virtual List<(string, UnityAction)> GetContextMenuItems()
    {
        var target = OutfitListUI.TargetOutfit;
        return
        [
             ("Use Animator",     () => target.SetAnimator(this.Descriptor))
            ,("Use Measurements", () => target.SetConfiguration(this.Descriptor))
            ,("Use Colliders",    () => target.SetColliderSource(this.Descriptor))
            ,("Activate Effects", () => target.SetEffect(this.Descriptor, true))
            ,("Disable Effects",  () => target.SetEffect(this.Descriptor, false))
        ];
    }
}