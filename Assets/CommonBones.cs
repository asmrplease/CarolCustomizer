using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Assets;
public static class CommonBones
{
    static HashSet<string> Bones;
    public static bool Ready => Bones is not null;

    public static void SetCommonBones()
    {
        if (Ready) { Log.Error("tried to replace standard bone names"); return; }

        Bones = GameManager.manager
            .GetOutfit(Constants.Pyjamas)
            .transform
            .RecursiveFindTransform(x => x.name == Constants.PelvisBone)   
            .GetComponentsInChildren<Transform>()
            .Select(x => x.name)
            .ToHashSet();

        foreach (var bone in Constants.HairBones) { Bones.Remove(bone); }
        Log.Info($"Found {Bones.Count} of expected 60 standard bones.");
    }

    public static bool IsCommon(string name) => Bones.Contains(name);
}
