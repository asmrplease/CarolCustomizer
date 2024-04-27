using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CarolCustomizer.Utils;
using CarolCustomizer.Hooks;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Models.Accessories;

namespace CarolCustomizer.Behaviors.Carol;

public class SkeletonManager : IDisposable
{
    #region Static Fields
    public static Dictionary<string, Transform> CommonBones { get; private set; }
    public static void SetCommonBones()
    {
        if (CommonBones is not null) { Log.Error("tried to replace standard bone names"); return; }

        var pjs = GameManager.manager.GetOutfit(Constants.Pyjamas);
        if (!pjs) { Log.Debug("didn't find pjs"); return; }

        var pelvis = pjs.transform.RecursiveFindTransform(x => x.name == "CarolPelvis");
        if (!pelvis) { Log.Debug("didn't find pelvis"); return; }

        CommonBones = pelvis.SkeletonToList().ToDictionary(keySelector: x => x.name, elementSelector: x => x);
        Log.Info($"Found {CommonBones.Count} of expected 69 standard bones.");
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
        UnityEngine.Object.Destroy(faceCopier);
    }
    #endregion

    #region Update Target
    private void SetNewPelvis(PelvisWatchdog newPelvis)
    {
        targetPelvis = newPelvis;

        //var activeOutfits = outfitBoneDicts.Keys.ToList();
        var activeOutfits = playerManager.outfitManager.ActiveOutfits;
        liveStandardBones.Clear();
        outfitBoneDicts.Clear();

        liveStandardBones = targetPelvis.BoneData.StandardBones;
        FixMissingStandardBones();

        var head = liveStandardBones["Bn_CarolHead"]?
            .GetComponent<DynamicBone>();

        if (!head) { Log.Warning("didn't find dyn component on head");}
        headDynamicBone = head;
        foreach (var outfit in activeOutfits) { AddBespokeBones(outfit); }
        //headDynamicBone.RestartHairJiggle();
    }
    #endregion

    #region Public Interface
    public Transform[] Mount(LiveAccessory acc)
    {
        AddBespokeBones(acc.outfit);
        return GetLiveBoneArray(acc.outfit, acc.bones);
    }

    public Transform GetLiveStandardBone(string name) => liveStandardBones[name];
    #endregion

    #region Private Implementation
    private void AddBespokeBones(Outfit outfit)
    {
        if (outfitBoneDicts.ContainsKey(outfit)) return;
        Dictionary<string, Transform> boneDict = new();

        foreach (var bespokeBone in outfit.boneData.BespokeBones)
        {
            if (!bespokeBone.cleanedBone) { Log.Warning($"no cleanedbone, outfit: {outfit.DisplayName}   "); continue; }
            if (!bespokeBone.referenceBone.parent) { Log.Warning($"referencebone {bespokeBone.referenceBone.name} has no parent"); continue; }
            var newBone = InstantiateAt(bespokeBone.cleanedBone, bespokeBone.referenceBone.parent.name);
            if (!newBone) continue;

            foreach (var bone in newBone.SkeletonToList()) { boneDict[bone.name] = bone; }
        }

        outfitBoneDicts[outfit] = boneDict;
        headDynamicBone.RestartHairJiggle();
    }

    private Transform InstantiateAt(Transform objectToInstantiate, string parentName)
    {
        if (!liveStandardBones.ContainsKey(parentName))
        { Log.Error($"Could not find {objectToInstantiate.name}'s parent, {parentName}."); return null; }

        var parentBone = liveStandardBones[parentName];
        if (!parentBone) { Log.Error($"failed to find parent bone when instantiating {objectToInstantiate.name}"); return null; }

        var newBone = UnityEngine.Object.Instantiate(objectToInstantiate.gameObject, parentBone.transform).transform;
        newBone.DeCloneName();
        return newBone;
    }

    private Transform[] GetLiveBoneArray(Outfit outfit, Transform[] boneList)
    {
        Transform[] output = new Transform[boneList.Length];

        if (!outfitBoneDicts.ContainsKey(outfit)) { Log.Error($"Tried to get bones when {outfit.DisplayName} wasn't in custom bone table"); return output; }
        var bespokeBones = outfitBoneDicts[outfit];

        for (int i = 0; i < boneList.Length; i++)
        {
            if (!boneList[i]) continue;
            var boneName = boneList[i].name;
            if (liveStandardBones.ContainsKey(boneName)) { output[i] = liveStandardBones[boneName]; continue; }
            if (bespokeBones.ContainsKey(boneName)) { output[i] = bespokeBones[boneName]; continue; }
            Log.Warning($"Neither dict found {boneName}.");
        }
        return output;
    }

    private void FixMissingStandardBones()
    {
        var missingBones = CommonBones
            .Where(x => !liveStandardBones.ContainsKey(x.Key))
            .ToDictionary(x => x.Key, x => x.Value);
        var parents = missingBones
            .Where(x => !missingBones.Values.Contains(x.Value.parent));

        foreach (var bone in parents)
        {
            var newBone = InstantiateAt(bone.Value, bone.Value.transform.parent.name);
            liveStandardBones[newBone.name] = newBone;
            foreach (var child in newBone.SkeletonToList()) { liveStandardBones[child.name] = child; }
        }
    }
    #endregion
}
