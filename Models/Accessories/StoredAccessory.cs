using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Hooks;
using CarolCustomizer.Models.Outfits;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Models.Accessories;

public class StoredAccessory : AccessoryDescriptor
{
    public readonly Outfit outfit;
    public readonly SkinnedMeshRenderer referenceSMR;
    public string DisplayName;

    public StoredAccessory(Outfit outfit, SkinnedMeshRenderer smr)
        : base(smr, outfit.AssetName)
    {
        this.outfit = outfit;
        referenceSMR = smr;
        DisplayName = Name.Split('_').Last();
    }

    public LiveAccessory MakeLive(SkeletonManager skeleton, FaceCopier faceCopier, Transform folder)
    {
        return outfit.FaceDefinition.Invoke(this.referenceSMR) ?
            new LiveFace(this, skeleton,faceCopier, folder) :
            new LiveAccessory(this, folder);
    }
}
