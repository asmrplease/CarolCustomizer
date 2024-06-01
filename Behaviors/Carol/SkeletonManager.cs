using CarolCustomizer.Hooks;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Carol;

public class SkeletonManager : IDisposable
{
    #region Dependencies
    CarolInstance playerManager;
    public readonly FaceCopier faceCopier;
    #endregion

    #region Instance Variables
    PelvisWatchdog targetPelvis;
    Dictionary<string, Transform> liveStandardBones = new();
    Dictionary<Outfit, Dictionary<string, Transform>> outfitBoneDicts = new();
    MagicaManager MagicaManager;
    #endregion

    public Dictionary<string, Transform> GetBoneSet(Outfit outfit)
    {
        if (!outfitBoneDicts.TryGetValue(outfit, out var boneSet)) return null;
        Dictionary<string, Transform> results = new(liveStandardBones);
        results.AddRange(boneSet);
        return results;
    }

    #region Lifecycle
    public SkeletonManager(CarolInstance player, GameObject parent)
    {
        MagicaManager = new MagicaManager(this);
        playerManager = player;
        playerManager.SpawnEvent += MagicaManager.HandleNewPelvis;
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
        Log.Debug($"AssignLiveBones({acc.Name})");
        if (acc is null) { Log.Error("Requested bones for null accessory"); return; }
        outfitBoneDicts.TryGetValue(acc.outfit, out var bespokeDict);
        bespokeDict ??= AddBespokeBones(acc.outfit);

        var referenceBones = acc.bones;
        Transform[] liveBones = new Transform[referenceBones.Length];

        foreach (var i in Enumerable.Range(0, referenceBones.Length))
        {
            if (!referenceBones[i]) continue;
            string boneName = referenceBones[i].name;
            if (liveStandardBones.TryGetValue(boneName, out liveBones[i])) continue;
            if (bespokeDict      .TryGetValue(boneName, out liveBones[i])) continue;
        }

        liveStandardBones.TryGetValue(acc.RootBoneName, out var rootBone);
        if (!rootBone) bespokeDict.TryGetValue(acc.RootBoneName, out rootBone);
        rootBone ??= liveStandardBones["CarolPelvis"];

        acc.SetLiveBones(liveBones, rootBone);
        MagicaManager.HandleNewLiveAcc(acc);
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

        Log.Debug("SkeletonManager.SetNewPelvis() AddBespokeBones");
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

        outfitBoneDicts[outfit] = boneDict;
        MagicaManager.HandleNewOutfit(outfit);
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
