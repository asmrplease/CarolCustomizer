using CarolCustomizer.Behaviors;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CarolCustomizer.Models;
public class BoneData : MonoBehaviour
{
    [SerializeField]
    public List<Transform> allTransforms;

    [SerializeField]
    List<Transform> standardBones;

    public Dictionary<string, Transform> StandardBones => standardBones.ToDictionaryOverwrite(x => x.name);

    public List<BespokeBone> BespokeBones { get; private set; } = new();

    public BoneData Constructor()
    {
        allTransforms = this.transform.SkeletonToList();

        List<Transform> filteringList = new(this.allTransforms);
        if (SkeletonManager.CommonBones is null) { SkeletonManager.SetStandardBones(); }

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
