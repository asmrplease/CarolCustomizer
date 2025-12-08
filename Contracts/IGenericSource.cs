using CarolCustomizer.Models;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Models.Outfits;
using MagicaCloth2;
using System.Collections.Generic;
using UnityEngine;

namespace CarolCustomizer.Contracts;

public interface IMagicaProvider : ISourceDescriptor, IBoneProvider
{
    List<MagicaCapsuleCollider> GetColliders();
    List<MagicaCloth> GetBoneCloths();
}

public interface IModelProvider : ISourceDescriptor, IBoneProvider
{
    StoredAccessory GetAccessory(AccessoryDescriptor accessory);
    List<StoredAccessory> GetAccessories();
    IInstantiable GetInstantiable(AccessoryDescriptor accessory);
}

public interface IMaterialProvider : ISourceDescriptor
{
    MaterialDescriptor GetMaterial(MaterialDescriptor material);
    List<MaterialDescriptor> GetMaterials();
}

public interface IEffectProvider : ISourceDescriptor
{
    List<OutfitEffect> GetEffects();
}

public interface IBoneProvider : ISourceDescriptor
{
    List<Transform> GetBespokeBones();
}

public interface IConfigProvider : ISourceDescriptor
{
    RuntimeAnimatorController GetAnimator();
    ModelData GetConfiguration();
}

public interface ISourceDescriptor
{
    SourceDescriptor Descriptor { get; }
}

public interface IGenericSource : 
    ISourceDescriptor, 
    IModelProvider,
    IMaterialProvider,
    IMagicaProvider,
    IBoneProvider,
    IConfigProvider,
    IEffectProvider
{ }
