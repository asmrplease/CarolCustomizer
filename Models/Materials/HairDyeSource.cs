using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using MagicaCloth2;
using Onirism.Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CarolCustomizer.Models.Materials;
public class HairDyeSource : IAccessorySource
{
    SourceDescriptor descriptor = new(Constants.HairDyeSourceName, SourceType.Hair);
    Dictionary<MaterialDescriptor, MaterialDescriptor> hairDyes = [];

    public HairDyeSource(List<HairDye> dyes)
    {
        dyes.Select(x => new MaterialDescriptor(x.material, this.descriptor))
            .ForEach(x => hairDyes.TryAdd(x, x));
    }

    SourceDescriptor IAccessorySource.Descriptor => descriptor;

    List<StoredAccessory> IAccessorySource.GetAccessories() => [];

    StoredAccessory IAccessorySource.GetAccessory(AccessoryDescriptor accessory) => null;

    RuntimeAnimatorController IAccessorySource.GetAnimator() => null;

    List<Transform> IAccessorySource.GetBespokeBones() => [];

    List<MagicaCloth> IAccessorySource.GetBoneCloths() => [];

    List<MagicaCapsuleCollider> IAccessorySource.GetColliders() => [];

    ModelData IAccessorySource.GetConfiguration() => null;

    List<OutfitEffect> IAccessorySource.GetEffects() => [];

    IInstantiable IAccessorySource.GetInstantiable(AccessoryDescriptor accessory) => null;

    public MaterialDescriptor GetMaterial(MaterialDescriptor material)
    {
        hairDyes.TryGetValue(material, out var result);
        return result;
    }

    List<MaterialDescriptor> IAccessorySource.GetMaterials()
    {
        Log.Warning("Someone asked for ALL of the hairdyes for some reason.");
        return hairDyes.Values.ToList();
    }
}
