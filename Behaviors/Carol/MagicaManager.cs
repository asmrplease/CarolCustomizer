using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using MagicaCloth2;
using MonoMod.Utils;
using Slate;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Carol;
internal class MagicaManager
{
    Dictionary<LiveAccessory, MagicaCloth> LiveCloths = new();

    SkeletonManager skeleton;

    List<MagicaCloth> processing = new();
    PelvisWatchdog targetPelvis;
    Dictionary<AccessoryDescriptor, MagicaCloth> MeshClothAccs = new();
    Dictionary<Outfit, MagicaCloth> BoneCloths = new();
    

    public MagicaManager(SkeletonManager skeleton)
    {
        this.skeleton = skeleton;
    }

    public void HandleNewPelvis(PelvisWatchdog newPelvis)
    {
        Log.Debug("magicamanager.handleNewPelvis()");
        //if (targetPelvis == newPelvis) return;

        targetPelvis = newPelvis;
        MeshClothAccs.Clear();
        Log.Debug("HandlePelvis done.");
    }

    public void HandleNewOutfit(Outfit outfit)
    {
        Log.Info($"MagicaManager.HandleNewOutfit({outfit.DisplayName}");
        if (!targetPelvis) { Log.Warning("MagicaManager had no pelvis during HandleNewOutfit"); return; }
        var magicas = outfit
            .compData
            .magicaCloths;
        if (magicas is null || !magicas.Any()) { Log.Warning($"{outfit.DisplayName} had no magica component"); return; }

        foreach (var magica in magicas)
        {
            switch (magica.SerializeData.clothType)
            {
                case ClothProcess.ClothType.BoneCloth:
                    Log.Debug("BoneCloth");
                    if (BoneCloths.TryGetValue(outfit, out var existingMagica) && existingMagica)
                    {
                        GameObject.DestroyImmediate(existingMagica.gameObject);
                    }  
                    
                    var liveMagica = GameObject.Instantiate(magica, targetPelvis.transform.parent);
                    liveMagica.name = outfit.DisplayName + " BoneCloth";
                    targetPelvis.DisableAnimator();
                    liveMagica.ReplaceTransform(skeleton.GetBoneSet(outfit));
                    liveMagica.SetParameterChange();
                    processing.Add(liveMagica);
                    var buildGuid = Guid.NewGuid();
                    Log.Debug(buildGuid.ToString());
                    liveMagica.OnBuildComplete += (x) => HandleBuildComplete(x, liveMagica, buildGuid);
                    liveMagica.BuildAndRun();
                    BoneCloths[outfit] = liveMagica;
                    magica.gameObject.SetActive(true);
                    break;
                case ClothProcess.ClothType.MeshCloth:
                    var smrs = magica
                        .SerializeData
                        .sourceRenderers
                        .Where(x => x.GetType() == typeof(SkinnedMeshRenderer))
                        .Select(x =>
                            new AccessoryDescriptor(
                                x as SkinnedMeshRenderer,
                                outfit.AssetName))
                        .ToDictionary(
                            x => x,
                            x => magica);
                    MeshClothAccs.AddRange(smrs);
                    break;
                case ClothProcess.ClothType.BoneSpring:
                    Log.Warning($"{outfit.DisplayName} has an unhandled bonespring component");
                    break;
            }
        }
    }

    public void HandleNewLiveAcc(LiveAccessory acc)
    {
        if (!MeshClothAccs.TryGetValue(acc.storedAcc, out var referenceMagica)) return;
        Log.Debug($"HandleNewLiveAcc({acc.Name})");

        if (LiveCloths.TryGetValue(acc, out var existingMagica) && existingMagica)
        {
            if (existingMagica.isActiveAndEnabled)
            {
                //Log.Error("Tried to replace active magica cloth!");
                //GameObject.DestroyImmediate()
                //return;
            }
            GameObject.DestroyImmediate(existingMagica.gameObject);
        }

        if (!acc.isActive) return;

        targetPelvis.DisableAnimator();
        MeshClothAccs.Remove(acc.storedAcc);
        //acc.DEBUG_GET_SMR().GetAddComponent<SMRWatcher>();
        var boneDict = skeleton.GetBoneSet(acc.outfit);
        //boneDict[acc.Name] = acc.DEBUG_GET_SMR().transform;

        referenceMagica.gameObject.SetActive(false);

        var liveMagica = GameObject.Instantiate(referenceMagica, targetPelvis.transform.parent);
        liveMagica.SerializeData.cullingSettings.cameraCullingMode = CullingSettings.CameraCullingMode.Off;

        acc.AddToMagica(liveMagica);
        skeleton.AssignLiveBones(acc);
        liveMagica.SerializeData.colliderCollisionConstraint.colliderList.Clear();
        liveMagica.SerializeData.colliderCollisionConstraint.colliderList.AddRange(
            targetPelvis
            .GetComponentsInChildren<MagicaCapsuleCollider>(true)
            .ToList());
        liveMagica.name = acc.Name + " MeshCloth";
        liveMagica.ReplaceTransform(boneDict);
        liveMagica.SetParameterChange();
        var buildGuid = Guid.NewGuid();
        Log.Debug(buildGuid.ToString());
        liveMagica.OnBuildComplete += (x) => HandleBuildComplete(x, liveMagica, buildGuid);
        liveMagica.gameObject.SetActive(true);
        referenceMagica.gameObject.SetActive(true);
        LiveCloths[acc] = liveMagica;
        Log.Debug("Bone list:");
        acc.DEBUG_GET_SMR().bones.Where(x => x).ForEach(x=>Log.Debug(x.name));
    }

    void HandleBuildComplete(bool success, MagicaCloth component, Guid buildGuid)
    {
        processing.Remove(component);
        processing.RemoveAll(x => !x);
        Log.Info($"HandleBuildComplete({buildGuid}): {success}.");
        if (!targetPelvis) { Log.Warning("build completed after pelvis was destroyed"); return; }

        if (processing.Any()) return;
        Log.Info("enabling animator");
        targetPelvis.EnableAnimator();
    }
}
