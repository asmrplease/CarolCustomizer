using CarolCustomizer.Assets;
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

    Dictionary<string, Dictionary<string, Transform>> outfitBoneDicts = [];

    public SkeletonManager(CarolInstance player, GameObject parent)
    {
        if (!parent) Log.Error("SkeletonManager was provided a null parent");
        Log.Debug("SkeletonManager.Constructor()");
        MagicaManager = new MagicaManager(this);
        faceCopier = parent
            .AddComponent<FaceCopier>()
            .Constructor(player);
        player.SpawnEvent += MagicaManager.HandleNewPelvis;
        player.SpawnEvent += this.HandleNewPelvis;
    }

    void HandleNewPelvis(PelvisWatchdog newPelvis)
    {
        outfitBoneDicts
            .Values
            .SelectMany(dict => dict.Values)
            .Where(trans => trans && !CommonBones.IsCommon(trans.name))
            .Select(x => x.gameObject)
            .ToList()
            .ForEach(GameObject.Destroy);
        outfitBoneDicts.Clear();
        targetPelvis = newPelvis;
    }

    public void AssignLiveBones(LiveAccessory acc)
    {
        Log.Debug($"AssignLiveBones({acc.Name})");
        if (acc is null) { Log.Error("Requested bones for null accessory"); return; }

        var source = acc.Source == Constants.HairstyleSourceName ?
            acc.Name : acc.Source; //if this is a hairstyle, use the hairstyle name not the source
        //probbaly not a good system but i'm getting tired of this problem so we can fix it later if it matters

        var bespokeDict = GetAddBoneSet(acc.Source, acc.BespokeBones);
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

    public Dictionary<string, Transform> GetAddBoneSet(string source, List<Transform> bespokeBones)
    {
        Log.Debug($"AddBespokeBones({source})");
        if (outfitBoneDicts.TryGetValue(source, out var dict)) return dict;

        Log.Debug("Adding bones");
        Dictionary<string, Transform> boneDict = new(targetPelvis.BoneData.StandardBones);
        foreach (var bespokeBone in bespokeBones)
        {
            Transform parentBone;
            if (!bespokeBone.parent){ parentBone = boneDict["Bn_CarolHead"]; }
            else if (!targetPelvis.BoneData.StandardBones.TryGetValue(bespokeBone.parent.name, out parentBone))
            { Log.Error($"Could not find {bespokeBone.name}'s parent, {bespokeBone.parent.name}."); continue; }

            GameObject.Instantiate(bespokeBone.gameObject, parentBone.transform)
                .transform
                .DeCloneName()
                .AllChildTransforms()
                .ForEach(x => boneDict[x.name] = x);
        }
        outfitBoneDicts[source] = boneDict;
        //if (acc.outfit is not null) MagicaManager.HandleNewOutfit(acc.outfit);
        return boneDict;
    }

    public void Dispose() => GameObject.Destroy(faceCopier);
}