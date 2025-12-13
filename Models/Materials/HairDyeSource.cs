using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using MagicaCloth2;
using Onirism.Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace CarolCustomizer.Models.Materials;
public partial class HairDyeSource 
{
    SourceDescriptor ISourceDescriptor.Descriptor => descriptor;
    SourceDescriptor descriptor = new(Constants.HairDyeSourceName, SourceType.Hair);
    Dictionary<MaterialDescriptor, CCHairDye> hairDyes = [];
    PathDescriptor path = new("HairDyes", PathType.Convenience);

    public HairDyeSource(List<HairDye> dyes)
    {
        dyes.Select(x => new CCHairDye(x))
            .ForEach(x => hairDyes.TryAdd(x.MaterialDescriptor, x));
    }
}

//public partial class HairDyeSource : IPath
//{
//    PathDescriptor IPath.PathDescriptor => this.path;
//}

public partial class HairDyeSource : IListable
{
    Sprite IListable.Thumbnail => hairDyes?.FirstOrDefault().Value.HairDye.visual;

    string IListable.Header => "Hair Dyes";

    string IListable.Subheader => $"Count: {hairDyes.Count}";

    Color IListable.BaseColor => Constants.DefaultColor;

    Color IListable.HighlightColor => Constants.Highlight;

    IEnumerable<IListable> IListable.Children => this.hairDyes.Values;

    UnityAction<bool> IListable.OnToggle => null;

    bool IListable.Filter<T>(Predicate<T> predicate)
    {
        throw new NotImplementedException();
    }

    List<(string, UnityAction)> IContextMenuActions.GetContextMenuItems()
    {
        return [];
    }
}

public partial class HairDyeSource : IGenericSource
{
    MaterialDescriptor IMaterialProvider.GetMaterial(MaterialDescriptor descriptor)
    {
        hairDyes.TryGetValue(descriptor, out var dye);
        var result = dye.MaterialDescriptor;
        return result;
    }
    List<MaterialDescriptor> IMaterialProvider.GetMaterials()
    {
        Log.Warning("Someone asked for ALL of the hairdyes for some reason.");
        return hairDyes.Keys.ToList();
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