using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Assets;
public static class CommonBones
{
    static HashSet<string> Bones;
    public static bool Ready => Bones is not null;
    static List<string> HairBones = new() 
    { 
        "Carol_TAIL_TOP", 
        "Carol_TAIL_HIGH_MID", 
        "Carol_TAIL_LOW_MID", 
        "Carol_TAIL_END", 
        "Carol_ENDBone001", 
        "Carol_ENDBone001Bone001", 
        "Carol_ENDBone001Bone001Bone001", 
        "Carol_ENDBone001Bone001Bone001Bone001", 
        "Carol_ENDBone001Bone001Bone001Bone001Bone001" 
    };
    public static void SetCommonBones()
    {
        if (Ready) { Log.Error("tried to replace standard bone names"); return; }

        Bones = GameManager.manager
            .GetOutfit(Constants.Pyjamas)
            .transform
            .RecursiveFindTransform(x => x.name == "CarolPelvis")
            .GetComponentsInChildren<Transform>()
            .Select(x => x.name)
            .ToHashSet();

        foreach (var bone in HairBones) { Bones.Remove(bone); }
        Log.Info($"Found {Bones.Count} of expected 60 standard bones.");
    }

    public static bool IsCommon(string name) => Bones.Contains(name);
}
