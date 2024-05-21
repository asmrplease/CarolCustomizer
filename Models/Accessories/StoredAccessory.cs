using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Models.Outfits;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Models.Accessories;

public class StoredAccessory : AccessoryDescriptor
{
    public readonly Outfit outfit;
    public readonly SkinnedMeshRenderer referenceSMR;
    public string DisplayName => Name.Split('_').Last();

    public StoredAccessory(Outfit outfit, SkinnedMeshRenderer smr)
        : base(smr, outfit.AssetName)
    {
        this.outfit = outfit;
        referenceSMR = smr;
    }

    public LiveAccessory MakeLive(SkeletonManager skeleton, Transform folder)
    {
        return outfit.FaceDefinition.Invoke(this.referenceSMR) ?
            new LiveFace(this, skeleton, folder) :
            new LiveAccessory(this, skeleton, folder);
    }
}
