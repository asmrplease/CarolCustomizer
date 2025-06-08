using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Models.Outfits;

public class Outfit : IDisposable, IComparable<Outfit>, IEquatable<Outfit>
{
    #region Dependencies
    public Transform storedAsset { get; protected set; }
    #endregion

    #region Public Interface
    public string AssetName { get; protected set; }
    virtual public string DisplayName { get; private set; }
    virtual public Sprite Sprite => null;
    virtual public string Author => "Crimson Tales";

    virtual public RuntimeAnimatorController RuntimeAnimator => null;

    protected Dictionary<AccessoryDescriptor, StoredAccessory> AccDict = new();

    public List<StoredAccessory> Accessories => AccDict.Values.ToList();
    public List<OutfitEffect> Effects
        { get; protected set; }
        = new();
    public HashSet<MaterialDescriptor> MaterialDescriptors { get; private set; } = new();

    public PelvisWatchdog prefabWatchdog { get; private set; }
    public BoneData boneData => prefabWatchdog.BoneData;
    public CompData compData => prefabWatchdog.CompData;
    public MagiData magiData => prefabWatchdog.MagiData;
    virtual public Func<SkinnedMeshRenderer, bool> FaceDefinition => (x) => x.name == "tete";

    #endregion

    #region Lifecycle
    public Outfit(Transform storedAsset)
    {
        if (!storedAsset) { Log.Error("Outfit constructor was passed a null transform."); return; }

        this.storedAsset = storedAsset;
        AssetName = storedAsset.name;
        DisplayName = LocalizationIndex.index.GetLine(this.storedAsset.gameObject.name).Replace("CAROL_", "");
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
        prefabWatchdog = pelvis.gameObject.AddComponent<PelvisWatchdog>();
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
                    AssetName)
                );
        }
        foreach (var effect in compData.EffectGameObjects)
        {
            Effects.Add(
                new OutfitEffect(
                    effect.transform.GetAddressRelativeTo(prefabWatchdog.transform),
                    OutfitEffect.ComponentType.Component,
                    AssetName)
                );
        }

        Log.Debug($"{DisplayName} constructed.");
    }

    public StoredAccessory GetAccessory(AccessoryDescriptor descriptor)
    {
        AccDict.TryGetValue(descriptor, out StoredAccessory result);
        result ??= GetAccessory(descriptor.Name);
        return result;
    }

    public StoredAccessory GetAccessory(string name) => AccDict.Values.FirstOrDefault(x => x.Name == name);

    public void Dispose()
    {
        if (!prefabWatchdog) return;
        if (prefabWatchdog.BoneData) GameObject.DestroyImmediate(prefabWatchdog.BoneData);
        if (prefabWatchdog.CompData) GameObject.DestroyImmediate(prefabWatchdog.CompData);
        if (prefabWatchdog.MagiData) GameObject.DestroyImmediate(prefabWatchdog.MagiData);
        GameObject.DestroyImmediate(prefabWatchdog);
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
    #endregion
}
