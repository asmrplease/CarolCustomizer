using CarolCustomizer.Hooks;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Carol;

public class SkeletonManager : IDisposable
{
    public readonly FaceCopier faceCopier;
    MagicaManager MagicaManager;
    PelvisWatchdog targetPelvis;

    Dictionary<Outfit, Dictionary<string, Transform>> outfitBoneDicts = new();

    public SkeletonManager(CarolInstance player, GameObject parent)
    {
        MagicaManager = new MagicaManager(this);
        player.SpawnEvent += MagicaManager.HandleNewPelvis;
        player.SpawnEvent += HandleNewPelvis;

        faceCopier = parent.AddComponent<FaceCopier>();
        faceCopier.Constructor(player);
    }

    void HandleNewPelvis(PelvisWatchdog newPelvis)
    {
        Log.Info("SkeletonManager.SetNewPelvis()");
        foreach (var dict in outfitBoneDicts.Values.ToList())
        {
            dict.Values
            .Where(x => x && !targetPelvis.BoneData.StandardBones.ContainsKey(x.name))
            .Select(x => x.gameObject)
            .ToList()
            .ForEach(GameObject.Destroy);
        }
        outfitBoneDicts.Clear();
        targetPelvis = newPelvis;
    }

    public void AssignLiveBones(LiveAccessory acc)
    {
        Log.Debug($"AssignLiveBones({acc.Name})");
        if (acc is null) { Log.Error("Requested bones for null accessory"); return; }

        var bespokeDict = GetBoneSet(acc.outfit);
        Transform[] liveBones = new Transform[acc.bones.Length];
        foreach (int i in Enumerable.Range(0, acc.bones.Length))
        {
            if (!acc.bones[i]) continue;
            bespokeDict.TryGetValue(acc.bones[i].name, out liveBones[i]);
        }
        bespokeDict.TryGetValue(acc.RootBoneName, out var rootBone);
        rootBone ??= targetPelvis.BoneData.StandardBones["CarolPelvis"];
        acc.SetLiveBones(liveBones, rootBone);
        MagicaManager.HandleNewLiveAcc(acc);
    }

    public Dictionary<string, Transform> GetBoneSet(Outfit outfit)
    {
        outfitBoneDicts.TryGetValue(outfit, out var boneSet);
        return boneSet ?? AddBespokeBones(outfit);
    }

    public Dictionary<string, Transform> AddBespokeBones(Outfit outfit)
    {
        Log.Debug($"AddBespokeBones({outfit.DisplayName})");
        if (outfitBoneDicts.TryGetValue(outfit, out var dict)) return dict;

        Log.Debug("Adding bones");
        Dictionary<string, Transform> boneDict = new(targetPelvis.BoneData.StandardBones);
        foreach (var bespokeBone in outfit.boneData.BespokeBones)
        {
            if (!targetPelvis.BoneData.StandardBones.TryGetValue(bespokeBone.parent.name, out var parentBone))
            { Log.Error($"Could not find {bespokeBone.name}'s parent, {bespokeBone.parent.name}."); continue; }

            var newBone = GameObject.Instantiate(bespokeBone.gameObject, parentBone.transform).transform;
            newBone.DeCloneName();
            foreach (var bone in newBone.AllChildTransforms()) { boneDict[bone.name] = bone; }
        }

        outfitBoneDicts[outfit] = boneDict;
        MagicaManager.HandleNewOutfit(outfit);
        return boneDict;
    }

    public void Dispose() => GameObject.Destroy(faceCopier);
}