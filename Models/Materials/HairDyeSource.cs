using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using MagicaCloth2;
using Onirism.Gameplay;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Models.Materials;
public class HairDyeSource : IGenericSource
{
    SourceDescriptor ISourceDescriptor.Descriptor => descriptor;
    SourceDescriptor descriptor = new(Constants.HairDyeSourceName, SourceType.Hair);
    Dictionary<MaterialDescriptor, MaterialDescriptor> hairDyes = [];

    public HairDyeSource(List<HairDye> dyes)
    {
        dyes.Select(x => new MaterialDescriptor(x.material, this.descriptor))
            .ForEach(x => hairDyes.TryAdd(x, x));
    }

    public MaterialDescriptor GetMaterial(MaterialDescriptor material)
    {
        hairDyes.TryGetValue(material, out var result);
        return result;
    }

    List<MaterialDescriptor> IMaterialProvider.GetMaterials()
    {
        Log.Warning("Someone asked for ALL of the hairdyes for some reason.");
        return hairDyes.Values.ToList();
    }

    List<StoredAccessory> IModelProvider.GetAccessories() => [];

    StoredAccessory IModelProvider.GetAccessory(AccessoryDescriptor accessory) => null;

    RuntimeAnimatorController IConfigProvider.GetAnimator() => null;

    List<Transform> IBoneProvider.GetBespokeBones() => [];

    List<MagicaCloth> IMagicaProvider.GetBoneCloths() => [];

    List<MagicaCapsuleCollider> IMagicaProvider.GetColliders() => [];

    ModelData IConfigProvider.GetConfiguration() => null;

    List<OutfitEffect> IEffectProvider.GetEffects() => [];

    IInstantiable IModelProvider.GetInstantiable(AccessoryDescriptor accessory) => null;
}
