using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Contracts;
using CarolCustomizer.Hooks;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using MagicaCloth2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace CarolCustomizer.Models.Accessories;
public partial class StoredHair : AccessoryDescriptor
{
    public readonly string AssetName;
    public readonly string DisplayName;
    public readonly Hairstyle hairstyle;
    public readonly SkinnedMeshRenderer smr;
    readonly List<Transform> BespokeBones;
    readonly List<MagicaCloth> boneCloths = [];
    readonly List<MagicaCloth> meshCloths = [];

    SourceDescriptor ISourceDescriptor.Descriptor => this.Source;

    public StoredHair(Hairstyle hairstyle) :
        base(hairstyle.name, new SourceDescriptor(hairstyle.name, SourceType.Hair))
    {
        this.hairstyle = hairstyle;
        if (hairstyle.localizationName == "") hairstyle.localizationName = "Haircut_Powerhelmet";
        this.AssetName = hairstyle.name;
        this.DisplayName = LocalizationIndex.GetLine(hairstyle.localizationName);
        var smr = hairstyle.model as SkinnedMeshRenderer;
        var boneCopy = GameObject.Instantiate(hairstyle.transform.root);
        GameObject.DontDestroyOnLoad(boneCopy);
        boneCopy.GetComponentsInChildren<Component>()
            .Where(x => !x.GetType().IsAssignableFrom(typeof(Transform)))
            .ToList().ForEach(GameObject.Destroy);
        boneCopy.transform.ResetLocalPosRot();
        this.BespokeBones = [boneCopy];
        this.BespokeBones.ForEach(x => x.transform.localScale = Vector3.one);
        this.smr = hairstyle.model as SkinnedMeshRenderer;
        this.Materials = this.smr.materials
            .Select(x => new MaterialDescriptor(x, this.Source))
            .ToArray();
        var magicas = hairstyle.transform.root
            .GetComponentsInChildren<MagicaCloth>();
        magicas
            .Where(x => x.SerializeData.clothType == ClothProcess.ClothType.MeshCloth)
            .ForEach(meshCloths.Add);
        magicas
            .Where(x => x.SerializeData.clothType == ClothProcess.ClothType.BoneCloth)
            .ForEach(boneCloths.Add);
    }
}

public partial class StoredHair : IGenericSource
{
    List<StoredAccessory> IModelProvider.GetAccessories() => [];

    StoredAccessory IModelProvider.GetAccessory(AccessoryDescriptor accessory)
    {
        Log.Warning($"Requested a StoredAccessory from StoredHair but this isn't implmented");
        return null;
    }

    public List<Transform> GetBespokeBones() => BespokeBones;

    List<MagicaCloth> IMagicaProvider.GetBoneCloths() => this.boneCloths;
    public List<MagicaCloth> GetMeshCloths() => this.meshCloths;

    MaterialDescriptor IMaterialProvider.GetMaterial(MaterialDescriptor material) => null;

    List<MaterialDescriptor> IMaterialProvider.GetMaterials() => [];

    public LiveAccessory MakeLive(SkeletonManager skeleton, FaceCopier faceCopier, Transform folder)
    {
        return new LiveAccessory(this, folder);
    }

    public RuntimeAnimatorController GetAnimator() => null;

    List<OutfitEffect> IEffectProvider.GetEffects() => [];

    ModelData IConfigProvider.GetConfiguration() => null;

    List<MagicaCapsuleCollider> IMagicaProvider.GetColliders() => [];
    IInstantiable IModelProvider.GetInstantiable(AccessoryDescriptor accessory)
    {
        //if (accessory != this) return null;

        return this;
    }
}

public partial class StoredHair : IInstantiable
{
    AccessoryDescriptor IInstantiable.Descriptor => this;
    public LiveAccessory MakeLive(SkeletonManager skeleton, Transform folder)
    {
        return new LiveAccessory(this, folder);
    }
}

public partial class StoredHair : IPath
{
    PathDescriptor IPath.PathDescriptor => new("Hairstyles", PathType.Convenience);
} 

public partial class StoredHair : IListable
{
    Sprite IListable.Thumbnail => this.hairstyle.visual;

    string IListable.Header => this.DisplayName;

    string IListable.Subheader => "Hairstyle";

    Color IListable.BaseColor => Constants.DefaultColor;

    Color IListable.HighlightColor => Constants.Highlight;

    IEnumerable<IListable> IListable.Children => [new MutableModel(this)];

    UnityAction<bool> IListable.OnToggle => null;

    bool IListable.Filter<T>(Predicate<T> predicate)
    {
        throw new NotImplementedException();
    }

    List<(string, UnityAction)> IContextMenuActions.GetContextMenuItems()
    {
        throw new NotImplementedException();
    }
}