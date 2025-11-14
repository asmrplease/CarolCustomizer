using CarolCustomizer.Assets;
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
    readonly List<Transform> BespokeBones;
    List<MagicaCloth> boneCloths = [];
    List<MagicaCloth> meshCloths = [];
    

    SourceDescriptor IAccessorySource.Descriptor => this.Source;
    AccessoryDescriptor IInstantiable.Descriptor => this;

    public StoredHair(Hairstyle hairstyle) :
        base(hairstyle.name, new SourceDescriptor(hairstyle.name, SourceType.Hair))
    {
        this.hairstyle = hairstyle;
        this.AssetName = hairstyle.name;
        this.DisplayName = LocalizationIndex.GetLine(hairstyle.localizationName);
        var smr = hairstyle.model as SkinnedMeshRenderer;
        var boneCopy = GameObject.Instantiate(hairstyle.transform.root);
        //boneCopy.hideFlags = HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
        GameObject.DontDestroyOnLoad(boneCopy);
        boneCopy.GetComponentsInChildren<Component>()
            .Where(x => !x.GetType().IsAssignableFrom(typeof(Transform)))
            .ToList().ForEach(GameObject.Destroy);
        this.BespokeBones = [boneCopy];
        this.BespokeBones.ForEach(x => x.transform.localScale = Vector3.one);
        this.smr = hairstyle.model as SkinnedMeshRenderer;
        var magicas = hairstyle.transform.root
            .GetComponentsInChildren<MagicaCloth>();
        magicas
            .Where(x => x.SerializeData.clothType == ClothProcess.ClothType.MeshCloth)
            .ForEach(meshCloths.Add);
        magicas
            .Where(x => x.SerializeData.clothType == ClothProcess.ClothType.BoneCloth)
            .ForEach(boneCloths.Add);
    }
 
    //static Transform GetFolder()
    //{
    //    if (CCPlugin.CoroutineRunner.transform.Find("HairBoneFolder") is Transform folder) return folder;
    //    var newFolder = new GameObject("HairBoneFolder");
    //    newFolder.transform.parent = CCPlugin.CoroutineRunner.transform;
    //    return newFolder.transform;
    //}

    public LiveAccessory MakeLive(SkeletonManager skeleton, Transform folder)
    {
        return new LiveAccessory(this, folder);
    }

    List<StoredAccessory> IAccessorySource.GetAccessories() => [];

    StoredAccessory IAccessorySource.GetAccessory(AccessoryDescriptor accessory)
    {
        Log.Warning($"Requested a StoredAccessory from StoredHair but this isn't implmented");
        return null;
    }

    public List<Transform> GetBespokeBones() => BespokeBones;

    List<MagicaCloth> IAccessorySource.GetBoneCloths() => this.boneCloths;
    public List<MagicaCloth> GetMeshCloths() => this.meshCloths;

    MaterialDescriptor IAccessorySource.GetMaterial(MaterialDescriptor material) => null;

    List<MaterialDescriptor> IAccessorySource.GetMaterials() => [];

    public LiveAccessory MakeLive(SkeletonManager skeleton, FaceCopier faceCopier, Transform folder)
    {
        return new LiveAccessory(this, folder);
    }

    public RuntimeAnimatorController GetAnimator() => null;

    List<OutfitEffect> IAccessorySource.GetEffects() => [];

    ModelData IAccessorySource.GetConfiguration() => null;

    List<MagicaCapsuleCollider> IAccessorySource.GetColliders() => [];

    IInstantiable IAccessorySource.GetInstantiable(AccessoryDescriptor accessory)
    {
        //if (accessory != this) return null;

        return this;
    }
}
