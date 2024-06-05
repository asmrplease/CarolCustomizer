using CarolCustomizer.Assets;
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
    public List<Transform> BespokeBones { get; private set; } = new();

    public BoneData Constructor()
    {
        allTransforms = transform.AllChildTransforms().ToList();

        List<Transform> filteringList = new(allTransforms);
        if (!CommonBones.Ready) { CommonBones.SetCommonBones(); }

        standardBones = filteringList
            .Where(x => CommonBones.IsCommon(x.name))
            .ToList();

        BespokeBones = filteringList
            .Except(standardBones)
            .Where(x=> 
                x.transform.parent
                && CommonBones.IsCommon(x.transform.parent.name))
            .ToList();

        return this;
    }
}
