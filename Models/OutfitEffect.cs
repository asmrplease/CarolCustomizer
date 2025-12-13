using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Contracts;
using CarolCustomizer.Hooks;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using ShaderControl;
using System;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Models;
public partial class OutfitEffect : AccessoryDescriptor
{
    public readonly string RelativePath;
    public readonly ComponentType Type;
    public OutfitEffect(string RelativePath, ComponentType type, SourceDescriptor source) : 
        base("__effects", source)
    {
        this.RelativePath = RelativePath;
        this.Type = type;
    }

    public enum ComponentType
    {
        Behavior = 0,
        Component = 1,
    }
}


public partial class OutfitEffect : IInstantiable
{
    AccessoryDescriptor IInstantiable.Descriptor => this;

    LiveAccessory IInstantiable.MakeLive(SkeletonManager skeleton, FaceCopier faceCopier, Transform folder)
    {
        throw new NotImplementedException();
        //var source = OutfitAssetManager.GetSource(this.Source);

        //var bones = skeleton.GetAddBoneSet(this.Source, source.GetBespokeBones());
        //var pelvis = bones["CarolPelvis"];
        //if (!pelvis) return null;
        //var transform = pelvis.transform.Find(this.RelativePath);
        //if (!transform) return null;

        //switch (this.Type)
        //{
        //    case OutfitEffect.ComponentType.Behavior:
        //        transform
        //            .GetComponents<Behaviour>()
        //            .ForEach(x => x.enabled = enabled);
        //        break;
        //    case OutfitEffect.ComponentType.Component:
        //        transform
        //            .gameObject
        //            .SetActive(enabled);
        //        break;
        //}
    }


}