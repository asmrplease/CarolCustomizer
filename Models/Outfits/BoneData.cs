using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Utils;
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

    public Dictionary<string, Transform> StandardBones => standardBones.ToDictionaryOverwrite(x => x.name);
    //public Dictionary<string, Transform> StandardBones => standardBones.ToDictionaryRename(x => x.name, x=> x.name += "2");
    public List<BespokeBone> BespokeBones { get; private set; } = new();

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

        BespokeBones = filteringList.Select(x => new BespokeBone(x)).ToList();
        return this;
    }
}
