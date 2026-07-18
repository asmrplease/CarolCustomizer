using CarolCustomizer.Utils;
using MagicaCloth2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MagicaCloth2.CullingSettings;
using static MagicaCloth2.ClothProcess;
using FuseBox.External.MagicaCloth2;
using CarolCustomizer.Assets;
using CarolCustomizer.Hooks.Watchdogs;

namespace CarolCustomizer.Models.Outfits;
public class MagiData : MonoBehaviour
{
    [SerializeField]
    public List<MagicaCloth> MagicaCloths;

    [SerializeField]
    public List<MagicaCloth> MeshCloths;

    [SerializeField]
    public List<MagicaCloth> BoneCloths;

    [SerializeField]
    public List<MagicaCloth> BoneSprings;

    [SerializeField]
    public List<MagicaCapsuleCollider> CapsuleColliders;

    [SerializeField]
    public MagicaClothCompanion ClothCompanion;

    public Dictionary<SkinnedMeshRenderer, List<MagicaCloth>> smrMeshClothDict = [];

    public MagiData Constructor()
    {
        MagicaCloths = transform
            .root
            .GetComponentsInChildren<MagicaCloth>(true)
            .ToList();

        MagicaCloths
            .Select(x => x.SerializeData.cullingSettings)
            .ForEach(x => x.cameraCullingMode = CameraCullingMode.Off);

        MeshCloths = MagicaCloths
            .Where(x => x.SerializeData.clothType == ClothType.MeshCloth)
            .ToList();

        MeshCloths//bonecloths don't have links to specific accessories? 
            .Select(meshCloth => (meshCloth, renderers: meshCloth.SerializeData.sourceRenderers))
            .ForEach(tup =>
            {
                tup.renderers
                    .Where(renderer => renderer.GetType() == typeof(SkinnedMeshRenderer))
                    .Select(renderer => renderer as SkinnedMeshRenderer)
                    .ForEach(smr =>
                    {
                        if  (smrMeshClothDict.TryGetValue(smr, out var list)) list.Add(tup.meshCloth);
                        else smrMeshClothDict.Add(smr, [tup.meshCloth]);
                    });
            });
        Log.Debug($"Found {smrMeshClothDict.Count()} smrs with meshcloths");

        BoneCloths = MagicaCloths
            .Where(x => x.SerializeData.clothType == ClothType.BoneCloth)
            .ToList();

        BoneSprings = MagicaCloths
            .Where(x => x.SerializeData.clothType == ClothType.BoneSpring)
            .ToList();

        CapsuleColliders = transform
            .GetComponentsInChildren<MagicaCapsuleCollider>(true)
            .ToList();
        if (CapsuleColliders.Count() == 0)
        {
            var myBones = this.GetComponent<PelvisWatchdog>().BoneData.StandardBones;
            OutfitAssetManager
                .GetPyjamas()
                .magiData
                .CapsuleColliders
                .Select(cc => (cc, found: myBones.TryGetValue(cc.name, out var bone), bone.gameObject))
                .Where(tup => tup.found)
                .ForEach(tup => tup.gameObject.AddComponent<MagicaCapsuleCollider>().CopyFrom(tup.cc));
            CapsuleColliders = transform
                .GetComponentsInChildren<MagicaCapsuleCollider>(true)
                .ToList();
        }

        ClothCompanion = transform
            .parent?.parent?.parent?
            .GetComponent<MagicaClothCompanion>();

        return this;
    }
}
