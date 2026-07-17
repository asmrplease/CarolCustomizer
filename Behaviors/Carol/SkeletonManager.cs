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
    Dictionary<SourceDescriptor, Dictionary<string, Transform>> outfitBoneDicts = [];
    PelvisWatchdog pelvis;
    public event Action<LiveAccessory> OnLiveBonesAssigned;

    public void HandleNewPelvis(PelvisWatchdog newPelvis)
    {
        Log.Debug("SkeletonManager.HandleNewPelvis()");
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

    public void AssignLiveBones(LiveAccessory acc, bool notify = true)
    {
        Log.Debug($"AssignLiveBones({acc.Name})");
        if (acc is null) { Log.Error("Requested bones for null accessory"); return; }

        var source = OutfitAssetManager.GetSource(acc.Source);
        //var source = acc.Source == Constants.HairstyleSourceName ?
        //    acc.Name : acc.Source; //if this is a hairstyle, use the hairstyle name not the source
        //probbaly not a good system but i'm getting tired of this problem so we can fix it later if it matters

        var bespokeDict = GetAddBoneSet(acc.Source, acc.BespokeBones);
        Transform[] liveBones = new Transform[acc.bones.Length];
        foreach (int i in Enumerable.Range(0, acc.bones.Length))
        {
            if (!acc.bones[i]) continue;
            bespokeDict.TryGetValue(acc.bones[i].name, out liveBones[i]);
        }
        bespokeDict.TryGetValue(acc.RootBoneName, out var rootBone);
        rootBone ??= pelvis.BoneData.StandardBones[Constants.Pelvis];
        acc.SetLiveBones(liveBones, rootBone);
        if (notify) OnLiveBonesAssigned?.Invoke(acc);
    }

    public Dictionary<string, Transform> GetAddBoneSet(SourceDescriptor source, List<Transform> bespokeBones)
    {
        if (source is null) { Log.Error("GetAddBoneSet was giving a null source string"); return null; }
        if (bespokeBones is null) { Log.Error("GetAddBoneSet was given a null list of bones"); return null; }

        Log.Debug($"GetAddBoneSet({source})");
        if (outfitBoneDicts.TryGetValue(source, out var dict)) { Log.Debug("Returning cached bone dict"); return dict; }

        Log.Debug($"Adding bone set for {source}");
        Dictionary<string, Transform> boneDict = new(pelvis.BoneData.StandardBones);
        foreach (var bespokeBone in bespokeBones)
        {
            Transform parentBone;
            if (!bespokeBone.parent){ parentBone = boneDict[Constants.HeadBone]; }
            else if (!pelvis.BoneData.StandardBones.TryGetValue(bespokeBone.parent.name, out parentBone))
            if (!parentBone) { Log.Error($"Could not find {bespokeBone.name}'s parent, {bespokeBone.parent.name}."); continue; }

            Log.Info($"Instantiating bone {bespokeBone.name} on {parentBone.name}");
            GameObject.Instantiate(bespokeBone.gameObject, parentBone.transform)
                .transform
                .DeCloneName()
                .AllChildTransforms()
                .ForEach(x => boneDict[x.name] = x);
        }
        outfitBoneDicts[source] = boneDict;
        return boneDict;
    }
}