using CarolCustomizer.Utils;
using MagicaCloth2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MagicaCloth2.CullingSettings;
using static MagicaCloth2.ClothProcess;
using FuseBox.External.MagicaCloth2;

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

    public MagiData Constructor()
    {
        MagicaCloths = transform
            .parent
            .GetComponentsInChildren<MagicaCloth>(true)
            .ToList();

        MagicaCloths
            .Select(x =>
                x.SerializeData.cullingSettings)
            .ForEach(x =>
                x.cameraCullingMode = CameraCullingMode.Off);

        MeshCloths = MagicaCloths
            .Where(x => 
                x.SerializeData.clothType == ClothType.MeshCloth)
            .ToList();

        BoneCloths = MagicaCloths
            .Where(x =>
                x.SerializeData.clothType == ClothType.BoneCloth)
            .ToList();

        BoneSprings = MagicaCloths
            .Where(x =>
                x.SerializeData.clothType == ClothType.BoneSpring)
            .ToList();

        CapsuleColliders = transform
            .GetComponentsInChildren<MagicaCapsuleCollider>(true)
            .ToList();

        ClothCompanion = transform
            .parent?.parent?.parent?
            .GetComponent<MagicaClothCompanion>();

        return this;
    }
}
