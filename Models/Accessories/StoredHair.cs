using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Models.Accessories;
public class StoredHair : AccessoryDescriptor
{
    public readonly string AssetName;
    public readonly string DisplayName;
    public readonly Hairstyle hairstyle;
    public readonly SkinnedMeshRenderer smr;
    public readonly List<Transform> BespokeBones;

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
}
