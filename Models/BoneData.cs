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

    public List<BespokeBone> BespokeBones { get; private set; }

    public BoneData Constructor()
    {
        allTransforms = this.transform.SkeletonToList();

        List<Transform> filteringList = new(this.allTransforms);

        if (SkeletonManager.CommonBones is null) { SkeletonManager.SetStandardBones(); }

        standardBones = filteringList.Where(x => SkeletonManager.CommonBones.Keys.Contains(x.name)).ToList();

        //FixMissingStandardBones();

        filteringList = filteringList
            .Except(standardBones)
            .ToList();

        //remove all bones that are children of a non-standard bone
        filteringList.RemoveAll
            (x => !SkeletonManager.CommonBones.Keys.Contains(x.transform.parent.name));

        BespokeBones = filteringList.Select(x => new BespokeBone(x)).ToList();
        Log.Debug("BoneData construction complete.");
        return this;
    }

    //TODO: Move this back to skeletonManager, we can't instantiate them ahead of time
    private void FixMissingStandardBones()
    {
        var existingStandard = StandardBones;

        var missingBones = SkeletonManager.CommonBones
            .Where(x => !existingStandard.ContainsKey(x.Key))
            .ToDictionary(x => x.Key, x => x.Value);
        var parents = missingBones
            .Where(x => !missingBones.Values.Contains(x.Value.parent));

        foreach (var bone in parents)
        {
            Transform newBone = InstantiateAt(bone.Value, bone.Value.transform.parent.name);
            standardBones.Add(newBone);
            foreach (var child in newBone.SkeletonToList()) { standardBones.Add(child); }
        }
    }

    private Transform InstantiateAt(Transform objectToInstantiate, string parentName)
    {
        var existingStandard = StandardBones;
        if (!existingStandard.ContainsKey(parentName))
        { Log.Error($"Could not find {objectToInstantiate.name}'s parent, {parentName}."); return null; }

        var parentBone = existingStandard[parentName];
        if (!parentBone) { Log.Error($"failed to find parent bone when instantiating {objectToInstantiate.name}"); return null; }

        var newBone = GameObject.Instantiate(objectToInstantiate.gameObject, parentBone.transform).transform;
        newBone.DeCloneName();
        return newBone;
    }
}
