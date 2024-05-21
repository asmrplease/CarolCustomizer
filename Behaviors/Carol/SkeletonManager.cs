﻿using CarolCustomizer.Hooks;
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
    #region Static Fields
    public static Dictionary<string, Transform> CommonBones { get; private set; }
    static List<string> HairBones = new() { "Carol_TAIL_TOP", "Carol_TAIL_HIGH_MID", "Carol_TAIL_LOW_MID", "Carol_TAIL_END", "Carol_ENDBone001", "Carol_ENDBone001Bone001", "Carol_ENDBone001Bone001Bone001", "Carol_ENDBone001Bone001Bone001Bone001", "Carol_ENDBone001Bone001Bone001Bone001Bone001" };
    public static void SetCommonBones()
    {
        if (CommonBones is not null) { Log.Error("tried to replace standard bone names"); return; }

        CommonBones = GameManager.manager
            .GetOutfit(Constants.Pyjamas)
            .transform
            .RecursiveFindTransform(x => x.name == "CarolPelvis")
            .SkeletonToList()
            .ToDictionary(keySelector: x => x.name, elementSelector: x => x);
        foreach (var bone in HairBones) { CommonBones.Remove(bone); }
        Log.Info($"Found {CommonBones.Count} of expected 60 standard bones.");
    }
    #endregion

    #region Dependencies
    CarolInstance playerManager;
    public readonly FaceCopier faceCopier;
    #endregion

    #region Instance Variables
    PelvisWatchdog targetPelvis;
    DynamicBone headDynamicBone;
    Dictionary<string, Transform> liveStandardBones = new();
    Dictionary<Outfit, Dictionary<string, Transform>> outfitBoneDicts = new();
    #endregion

    #region Lifecycle
    public SkeletonManager(CarolInstance player, GameObject parent)
    {
        playerManager = player;
        playerManager.SpawnEvent += SetNewPelvis;

        faceCopier = parent.AddComponent<FaceCopier>();
        faceCopier.Constructor(playerManager);
    }

    public void Dispose()
    {
        playerManager.SpawnEvent -= SetNewPelvis;
        GameObject.Destroy(faceCopier);
    }
    #endregion

    #region Public Interface
    public void AssignLiveBones(LiveAccessory acc)
    {
        if (acc is null) { Log.Error("Requested bones for null accessory"); return; }
        outfitBoneDicts.TryGetValue(acc.outfit, out var bespokeDict);
        bespokeDict ??= AddBespokeBones(acc.outfit);

        var referenceBones = acc.bones;
        Transform[] liveBones = new Transform[referenceBones.Length];

        foreach (var i in Enumerable.Range(0, referenceBones.Length))
        {
            if (!referenceBones[i]) continue;
            string boneName = referenceBones[i].name;
            if (bespokeDict.ContainsKey(boneName))       { liveBones[i] = bespokeDict[boneName]; continue; }
            if (liveStandardBones.ContainsKey(boneName)) { liveBones[i] = liveStandardBones[boneName]; continue; }
        }

        Transform rootBone = null;
        if (liveStandardBones.ContainsKey(acc.RootBoneName)) rootBone = liveStandardBones[acc.RootBoneName];
        if (bespokeDict.ContainsKey(acc.RootBoneName)) rootBone = bespokeDict[acc.RootBoneName];
        rootBone ??= liveStandardBones["CarolPelvis"];

        acc.SetLiveBones(liveBones, rootBone);
    }
    #endregion

    #region Private Implementation
    void SetNewPelvis(PelvisWatchdog newPelvis)
    {
        Log.Info("SkeletonManager.SetNewPelvis()");
        if (newPelvis == targetPelvis) { Log.Debug("SkeletonManager was given its existing pelvis"); return; }

        outfitBoneDicts
            .Keys
            .ToList()
            .ForEach(RemoveBespokeBones);
        outfitBoneDicts.Clear();

        targetPelvis = newPelvis;
        liveStandardBones = targetPelvis.BoneData.StandardBones;
        //TODO: tell magica about new bones?


        Log.Debug("SkeletonManager.SetNewPelvis() AddBespokeBones");
        playerManager
            .outfitManager
            .ActiveOutfits
            .ForEach(x => AddBespokeBones(x));
    }

    public Dictionary<string, Transform> AddBespokeBones(Outfit outfit)
    {
        Log.Debug($"AddBespokeBones({outfit.DisplayName})");
        if (outfitBoneDicts.TryGetValue(outfit, out var dict)) return dict;

        Log.Debug("Adding bones");
        Dictionary<string, Transform> boneDict = new();
        foreach (var bespokeBone in outfit.boneData.BespokeBones)
        {
            if (!bespokeBone.parent) { Log.Warning($"BespokeBone {bespokeBone.name} has no parent"); continue; }
            string parentName = bespokeBone.parent.name;
            var newBone = InstantiateAt(bespokeBone, parentName);
            if (!newBone) continue;

            foreach (var bone in newBone.SkeletonToList()) { boneDict[bone.name] = bone; }
        }

        var newHairRoots = outfit
            .boneData
            .MagicaBones
            .Where(x => 
                boneDict.ContainsKey(x.name))
            .Select(x =>
                boneDict[x.name]);

        Log.Debug("New Hair roots:");
        newHairRoots.ForEach(x => Log.Debug(x.name));

        targetPelvis
            .CompData
            .magicaCloth
            .SerializeData
            .rootBones
            .AddRange(newHairRoots);

        targetPelvis
            .CompData
            .magicaCloth
            .SetParameterChange();

        outfitBoneDicts[outfit] = boneDict;

        return boneDict;
    }

    

    void RemoveBespokeBones(Outfit outfit)
    {
        if (!outfitBoneDicts.TryGetValue(outfit, out var dict)) return;

        dict.Values
            .Where(x => x)
            .Select(x => x.gameObject)
            .ToList()
            .ForEach(GameObject.Destroy);
    }

    Transform InstantiateAt(Transform objectToInstantiate, string parentName)
    {
        if (!liveStandardBones.ContainsKey(parentName))
        { Log.Error($"Could not find {objectToInstantiate.name}'s parent, {parentName}."); return null; }

        var parentBone = liveStandardBones[parentName];
        if (!parentBone) { Log.Error($"failed to find parent bone when instantiating {objectToInstantiate.name}"); return null; }

        var newBone = GameObject.Instantiate(objectToInstantiate.gameObject, parentBone.transform).transform;
        newBone.DeCloneName();
        return newBone;
    }
    #endregion
}
