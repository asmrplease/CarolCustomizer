using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using MagicaCloth2;
using MonoMod.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Carol;
internal class MagicaManager
{
    //outfitmanager decides to enable an outfit with a cloth component
    //we add a check for the targeted accessories to the accessory instantiation process
    //when that storedacc is enabled, give it bones then perform it's cloth setup.

    SkeletonManager skeleton;

    List<MagicaCloth> processing = new();
    PelvisWatchdog targetPelvis;
    Dictionary<AccessoryDescriptor, MagicaCloth> MeshClothAccs = new();

    public MagicaManager(SkeletonManager skeleton)
    {
        this.skeleton = skeleton;
    }

    public void HandleNewPelvis(PelvisWatchdog newPelvis)
    {
        Log.Debug("magicamanager.handleNewPelvis()");
        if (targetPelvis == newPelvis) return;

        targetPelvis = newPelvis;
        MeshClothAccs.Clear();
        Log.Debug("HandlePelvis done.");
    }

    public void HandleNewOutfit(Outfit outfit)
    {
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
                    var liveMagica = GameObject.Instantiate(magica, targetPelvis.transform.parent);
                    liveMagica.ReplaceTransform(skeleton.GetBoneSet(outfit));
                    liveMagica.SetParameterChange();
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
        
        if (targetPelvis.CompData?.Animator) targetPelvis.CompData.Animator.enabled = false;
        MeshClothAccs.Remove(acc.storedAcc);
        var boneDict = skeleton.GetBoneSet(acc.outfit);

        referenceMagica.gameObject.SetActive(false);

        var liveMagica = GameObject.Instantiate(referenceMagica, targetPelvis.transform.parent);
        liveMagica.SerializeData.cullingSettings.cameraCullingMode = CullingSettings.CameraCullingMode.Off;

        acc.AddToMagica(liveMagica);
        liveMagica.SerializeData.colliderCollisionConstraint.colliderList.Clear();
        liveMagica.SerializeData.colliderCollisionConstraint.colliderList.AddRange(
            targetPelvis
            .GetComponentsInChildren<MagicaCapsuleCollider>(true)
            .ToList());

        liveMagica.ReplaceTransform(boneDict);
        liveMagica.SetParameterChange();
        liveMagica.OnBuildComplete += (x) => HandleBuildComplete(x, liveMagica);
        liveMagica.gameObject.SetActive(true);
        referenceMagica.gameObject.SetActive(true);
    }

    IEnumerable StartProcessing()
    {
        var anim = targetPelvis.CompData.Animator;
        anim.SetLayerWeight(anim.GetLayerIndex("RangedHoldOneHanded"), 0f);
        anim.SetLayerWeight(anim.GetLayerIndex("RangedHoldTwoHanded"), 0f);
        anim.SetLayerWeight(anim.GetLayerIndex("PistolWalk"), 0f);
        anim.SetLayerWeight(anim.GetLayerIndex("BubblerWalk"), 0f);
        anim.SetLayerWeight(anim.GetLayerIndex("BazookaWalk"), 0f);
        anim.SetLayerWeight(anim.GetLayerIndex("TwoHandWalk"), 0f);
        anim.SetLayerWeight(anim.GetLayerIndex("StaffWalk"), 0f);
        anim.SetLayerWeight(anim.GetLayerIndex("DualWieldWalk"), 0f);
        anim.SetLayerWeight(anim.GetLayerIndex("SwordWieldWalk"), 0f);
        anim.SetLayerWeight(anim.GetLayerIndex("Zombieweapon"), 0f);
        anim.SetLayerWeight(anim.GetLayerIndex("Melee"), 0f);
        anim.enabled = false;
        yield return new WaitForEndOfFrame();
    }

    void HandleBuildComplete(bool success, MagicaCloth component)
    {
        processing.Remove(component);
        string smrName = component.SerializeData.sourceRenderers.FirstOrDefault()?.name ?? "null";
        Log.Info($"HandleBuildComplete({smrName}): {success}.");
        if (!targetPelvis) { Log.Warning("build completed after pelvis was destroyed"); return; }

        if (processing.Any()) return;

        if (targetPelvis.CompData?.Animator) targetPelvis.CompData.Animator.enabled = true;
    }
}
