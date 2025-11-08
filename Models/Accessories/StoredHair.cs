using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Contracts;
using CarolCustomizer.Hooks;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using MagicaCloth2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Models.Accessories;
public class StoredHair : AccessoryDescriptor, IAccessorySource, IInstantiable
{
    public readonly string AssetName;
    public readonly string DisplayName;
    public readonly Hairstyle hairstyle;
    public readonly SkinnedMeshRenderer smr;
    public readonly List<Transform> BespokeBones;

    SourceDescriptor IAccessorySource.Descriptor => this.Source;

    public StoredHair(Hairstyle hairstyle) :
        base(hairstyle.name, new SourceDescriptor(Constants.HairstyleSourceName, SourceType.Hair))
    {
        this.hairstyle = hairstyle;
        this.AssetName = hairstyle.name;
        this.DisplayName = LocalizationIndex.GetLine(hairstyle.localizationName);
        var smr = hairstyle.model as SkinnedMeshRenderer;
        this.BespokeBones = smr.rootBone.AllChildTransforms().ToList();
        this.smr = hairstyle.model as SkinnedMeshRenderer;
    }
    

    public LiveAccessory MakeLive(SkeletonManager skeleton, Transform folder)
    {
        return new LiveAccessory(this, folder);
    }

    List<StoredAccessory> IAccessorySource.GetAccessories()
    {
        return [];
    }

    StoredAccessory IAccessorySource.GetAccessory(AccessoryDescriptor accessory)
    {
        Log.Warning($"Requested a StoredAccessory from StoredHair but this isn't implmented");
        return null;
    }

    List<Transform> IAccessorySource.GetBespokeBones() => BespokeBones;

    List<MagicaCloth> IAccessorySource.GetBoneCloths() => [this.hairstyle.cloth];

    MaterialDescriptor IAccessorySource.GetMaterial(MaterialDescriptor material)
    {
        return null;
    }

    List<MaterialDescriptor> IAccessorySource.GetMaterials()
    {
        return [];
    }

    public LiveAccessory MakeLive(SkeletonManager skeleton, FaceCopier faceCopier, Transform folder)
    {
        return new LiveAccessory(this, folder);
    }

    public RuntimeAnimatorController GetAnimator() => null;

    List<OutfitEffect> IAccessorySource.GetEffects() => [];

    ModelData IAccessorySource.GetConfiguration() => null;

    List<MagicaCapsuleCollider> IAccessorySource.GetColliders() => [];
}
