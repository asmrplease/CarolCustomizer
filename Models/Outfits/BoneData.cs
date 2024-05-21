using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Utils;
using MagicaCloth2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Models.Outfits;
public class BoneData : MonoBehaviour
{
    [SerializeField]
    public List<Transform> allTransforms;

    [SerializeField]
    List<Transform> standardBones;

    [SerializeField]
    public List<Transform> MagicaBones;

    public Dictionary<string, Transform> StandardBones => standardBones.ToDictionaryOverwrite(x => x.name);
    public List<Transform> BespokeBones { get; private set; } = new();
    

    public BoneData Constructor()
    {
        allTransforms = transform.SkeletonToList();

        List<Transform> filteringList = new(allTransforms);
        if (SkeletonManager.CommonBones is null) { SkeletonManager.SetCommonBones(); }

        standardBones = filteringList.Where(x => SkeletonManager.CommonBones.Keys.Contains(x.name)).ToList();

        filteringList = filteringList
            .Except(standardBones)
            .ToList();

        filteringList.RemoveAll
            (x => !SkeletonManager.CommonBones.Keys.Contains(x.transform.parent.name));

        BespokeBones = filteringList.ToList();

        var magica = transform
            .parent
            .GetComponentInChildren<MagicaCloth>(true);

        if (magica)
        {
            MagicaBones = magica
                .SerializeData
                .rootBones;
        }
        MagicaBones ??= new();

        return this;
    }
}
