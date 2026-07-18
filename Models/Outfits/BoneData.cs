using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors;
using CarolCustomizer.Utils;
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
    //TODO: This feels really dumb, but for some reason things break when the values are cached

    public List<Transform> BespokeBones { get; private set; } = new();

    public BoneData Constructor()
    {
        allTransforms = transform.AllChildTransforms().ToList();

        List<Transform> filteringList = new(allTransforms);
        if (!CommonBones.Ready) { CommonBones.SetCommonBones(); }

        standardBones = filteringList
            .Where(x => CommonBones.IsCommon(x.name))
            .ToList();

        if (CommonBones.CommonBoneCount > standardBones.Count())
        {
            var pelvis = standardBones.FirstOrDefault(x => x.name == Constants.Pelvis);
            var boneNames = standardBones
                .Select(x => x.name);
            CommonBones.Bones
                .Except(boneNames)
                .ForEach(name => ArmatureNormalizer.FindAndRename(pelvis, name));
        }

        BespokeBones = filteringList
            .Except(standardBones)
            .Where(x=> 
                x.transform.parent
                && CommonBones.IsCommon(x.transform.parent.name))
            .ToList();

        return this;
    }

    public void MoveToRestPosition()
    {
        OutfitAssetManager
            .GetPyjamas().boneData
            .StandardBones
            .Select(kvp =>
                (found: this.StandardBones.TryGetValue(kvp.Key, out var result)
                , resting: kvp.Value
                , live: result))
            .Where(tup => tup.found)
            .ForEach(tup => tup.live.CopyFrom(tup.resting));
    }
}