using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Hooks;
using CarolCustomizer.Models.Outfits;
using MagicaCloth2;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Models.Accessories;

public class StoredAccessory : AccessoryDescriptor
{
    public readonly Outfit outfit;
    public readonly SkinnedMeshRenderer referenceSMR;
    public readonly MagicaCloth magica;
    public string DisplayName;

    public StoredAccessory(Outfit outfit, SkinnedMeshRenderer smr)
        : base(smr, new SourceDescriptor(outfit.AssetName, SourceType.Outfit))
    {
        this.outfit = outfit;
        referenceSMR = smr;
        DisplayName = Name.Split('_').Last();
        outfit.magiData.smrMeshClothDict.TryGetValue(smr, out this.magica);
    }

    public LiveAccessory MakeLive(SkeletonManager skeleton, FaceCopier faceCopier, Transform folder)
    {
        return outfit.FaceDefinition.Invoke(this.referenceSMR) ?
            new LiveFace(this, skeleton,faceCopier, folder) :
            new LiveAccessory(this, folder, this.magica);
    }
}
