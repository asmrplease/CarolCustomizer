using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Models.Outfits;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Models.Accessories;

public class StoredAccessory : AccessoryDescriptor
{
    public readonly Outfit outfit;//TODO: move this to an interface to prevent modification and allow for abstraction
    public readonly SkinnedMeshRenderer referenceSMR;
    public readonly bool isFace;
    public string DisplayName => Name.Split('_').Last();
    public string GetName() => Name;
    public string GetSource() => Source;

    public StoredAccessory(Outfit outfit, SkinnedMeshRenderer smr)
        : base(smr, outfit.AssetName)
    {
        this.outfit = outfit;
        referenceSMR = smr;
        isFace = smr.name == "tete";
    }

    public LiveAccessory BringLive(SkeletonManager skeleton, Transform folder)
    {
        if (isFace) return new LiveFace(this, skeleton, folder);
        return new LiveAccessory(this, skeleton, folder);
    }
}
