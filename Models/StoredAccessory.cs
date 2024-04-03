using CarolCustomizer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CarolCustomizer.Behaviors;

namespace CarolCustomizer.Models;

public class StoredAccessory : AccessoryDescriptor
{
    public readonly Outfit outfit;//TODO: move this to an interface to prevent modification and allow for abstraction
    public readonly SkinnedMeshRenderer referenceSMR;
    public readonly bool isFace;
    public string DisplayName => Name.Split('_').Last();
    public string GetName() => this.Name;
    public string GetSource() => this.Source;

    public StoredAccessory(Outfit outfit, SkinnedMeshRenderer smr)
        : base(smr.name, outfit.AssetName)
    {
        this.outfit = outfit;
        this.referenceSMR = smr;
        this.isFace = smr.name == "tete";

        Materials = new MaterialDescriptor[referenceSMR.materials.Length];

        int index = 0;
        foreach (var material in referenceSMR.materials)
        {
            Materials[index++] = new(material, this.outfit.AssetName, MaterialDescriptor.SourceType.AssetBundle);
        }
    }

    public LiveAccessory BringLive(SkeletonManager skeleton, Transform folder)
    {
        if (isFace) return new LiveFace(this, skeleton, folder);
        return new LiveAccessory(this, skeleton, folder);
    }
}
