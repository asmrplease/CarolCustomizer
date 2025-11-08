using CarolCustomizer.Models;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Models.Outfits;
using MagicaCloth2;
using System.Collections.Generic;
using UnityEngine;

namespace CarolCustomizer.Contracts
{
    public interface IAccessorySource
    {
        SourceDescriptor Descriptor { get; }
        StoredAccessory GetAccessory(AccessoryDescriptor accessory);
        List<StoredAccessory> GetAccessories();
        IInstantiable GetInstantiable(AccessoryDescriptor accessory);
        MaterialDescriptor GetMaterial(MaterialDescriptor material);
        List<MaterialDescriptor> GetMaterials();
        List<Transform> GetBespokeBones();
        List<MagicaCloth> GetBoneCloths();
        RuntimeAnimatorController GetAnimator();
        List<OutfitEffect> GetEffects();
        List<MagicaCapsuleCollider> GetColliders();
        ModelData GetConfiguration();
    }
}
