using CarolCustomizer.Assets;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Carol;

public class SkeletonManager
{
    PelvisWatchdog pelvis;
    Dictionary<Outfit, Dictionary<string, Transform>> outfitBoneDicts = [];
    public event Action<LiveAccessory> OnLiveBonesAssigned;
    public event Action<Outfit> OnOutfitBonesAdded;

    public void HandleNewPelvis(PelvisWatchdog newPelvis)
    {
        outfitBoneDicts
            .Values
            .SelectMany(dict => dict.Values)
            .Where(trans => trans && !CommonBones.IsCommon(trans.name))
            .Select(x => x.gameObject)
            .ToList()
            .ForEach(GameObject.Destroy);
        outfitBoneDicts.Clear();
        pelvis = newPelvis;
    }

    public void AssignLiveBones(LiveAccessory acc)
    {
        Log.Debug($"AssignLiveBones({acc.Name})");
        if (acc is null) { Log.Error("Requested bones for null accessory"); return; }

        var bespokeDict = GetAddBoneSet(acc.outfit);
        Transform[] liveBones = new Transform[acc.bones.Length];
        foreach (int i in Enumerable.Range(0, acc.bones.Length))
        {
            if (!acc.bones[i]) continue;
            bespokeDict.TryGetValue(acc.bones[i].name, out liveBones[i]);
        }
        bespokeDict.TryGetValue(acc.RootBoneName, out var rootBone);
        rootBone ??= pelvis.BoneData.StandardBones["CarolPelvis"];
        acc.SetLiveBones(liveBones, rootBone);
        OnLiveBonesAssigned?.Invoke(acc);
    }

    public Dictionary<string, Transform> GetAddBoneSet(Outfit outfit)
    {
        Log.Debug($"AddBespokeBones({outfit.DisplayName})");
        if (outfitBoneDicts.TryGetValue(outfit, out var dict)) return dict;

        Log.Debug("Adding bones");
        Dictionary<string, Transform> boneDict = new(pelvis.BoneData.StandardBones);
        foreach (var bespokeBone in outfit.boneData.BespokeBones)
        {
            Transform parentBone;
            if (!bespokeBone.parent){ parentBone = boneDict["Bn_CarolHead"]; }
            else if (!pelvis.BoneData.StandardBones.TryGetValue(bespokeBone.parent.name, out parentBone))
            { Log.Error($"Could not find {bespokeBone.name}'s parent, {bespokeBone.parent.name}."); continue; }

            GameObject.Instantiate(bespokeBone.gameObject, parentBone.transform)
                .transform
                .DeCloneName()
                .AllChildTransforms()
                .ForEach(x => boneDict[x.name] = x);
        }
        outfitBoneDicts[outfit] = boneDict;
        OnOutfitBonesAdded?.Invoke(outfit);
        return boneDict;
    }
}