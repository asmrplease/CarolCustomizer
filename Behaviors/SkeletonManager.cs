using Slate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CarolCustomizer.Models;
using CarolCustomizer.Utils;
using CarolCustomizer.Hooks;
using CarolCustomizer.Hooks.Watchdogs;

namespace CarolCustomizer.Behaviors;

public class SkeletonManager : IDisposable
{
    #region Static Fields
    public static Dictionary<string, Transform> CommonBones { get; private set; }
    public static void SetStandardBones()
    {
        if (CommonBones is not null) { Log.Error("tried to replace standard bone names"); return; }

        var pjs = GameManager.manager.GetOutfit(Constants.Pyjamas);
        if (!pjs) { Log.Debug("didn't find pjs"); return; }

        var pelvis = pjs.transform.RecursiveFindTransform(x => x.name == "CarolPelvis");
        if (!pelvis) { Log.Debug("didn't find pelvis"); return; }

        CommonBones = pelvis.SkeletonToList().ToDictionary(keySelector: x => x.name, elementSelector: x => x);
        Log.Info($"Found {CommonBones.Count} of 69 standard bones.");
    }
    #endregion

    #region Dependencies
    private CarolInstance playerManager;
    public readonly FaceCopier faceCopier;
    #endregion

    #region Instance Variables
    private PelvisWatchdog targetPelvis;
    private DynamicBone headDynamicBone;
    private Dictionary<string, Transform> liveStandardBones = new();
    private Dictionary<Outfit, Dictionary<string, Transform>> outfitBoneDicts = new();
    #endregion

    #region Lifecycle
    public SkeletonManager(CarolInstance player, GameObject parent)
    {
        this.playerManager = player;
        this.playerManager.SpawnEvent += SetNewPelvis;

        faceCopier = parent.AddComponent<FaceCopier>();
        faceCopier.Constructor(playerManager);
    }

    public void Dispose()
    {
        this.playerManager.SpawnEvent -= SetNewPelvis;
        GameObject.Destroy(faceCopier);
    } 
    #endregion

    #region Update Target
    private void SetNewPelvis(PelvisWatchdog newPelvis)
    {
        //Log.Debug("Changing SkeletonManager Target...");
        this.targetPelvis = newPelvis;        
        RebuildBones();

        var head = liveStandardBones["Bn_CarolHead"];
        if (!head) { Log.Warning("didn't find head bone!?"); return; }

        var headDyn = head.GetComponent<DynamicBone>();
        if (!headDyn) { Log.Warning("didn't find dyn component on head"); return; }

        headDynamicBone = headDyn;
        headDynamicBone.RestartHairJiggle();
    }

    private void RebuildBones()
    {
        liveStandardBones.Clear();
        outfitBoneDicts.Clear();
        
        liveStandardBones = this.targetPelvis.BoneData.StandardBones;

        //TODO: Remove dependency on pm & om
        HashSet<Outfit> activeOutfits = 
            new(this.playerManager.outfitManager.ActiveAccessories.Select(x => x.outfit));

        foreach (var outfit in activeOutfits) { AddBespokeBones(outfit); }
    }
    #endregion

    #region Public Interface
    public Transform[] Mount(LiveAccessory acc)
    {
        //Log.Debug($"Mount({acc.Name})");
        var siblingAcc = this.playerManager.outfitManager.ActiveAccessories.
            Any(x => x != acc && x.outfit == acc.outfit);
        if (!siblingAcc) { AddBespokeBones(acc.outfit); }

        return GetLiveBoneArray(acc.outfit, acc.bones);
    }

    public Transform GetLiveStandardBone(string name) => liveStandardBones[name];
    #endregion

    #region Private Implementation
    private void AddBespokeBones(Outfit outfit)
    {
        if (outfitBoneDicts.ContainsKey(outfit)) { return; }
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

        var newBone = GameObject.Instantiate(objectToInstantiate.gameObject, parentBone.transform).transform;
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
    #endregion
}
