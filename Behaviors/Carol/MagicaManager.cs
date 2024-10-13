using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using MagicaCloth2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Carol;
internal class MagicaManager(SkeletonManager skeleton)
{
    Dictionary<LiveAccessory, MagicaCloth> LiveCloths = new();

    SkeletonManager skeleton = skeleton;
    PelvisWatchdog targetPelvis;

    List<MagicaCloth> processing = new();
    Dictionary<AccessoryDescriptor, MagicaCloth> MeshClothAccs = new();
    List<MagicaCloth> BoneCloths = new();

    public void HandleNewPelvis(PelvisWatchdog newPelvis)
    {
        Log.Debug("magicamanager.handleNewPelvis()");
        targetPelvis = newPelvis;
        MeshClothAccs.Clear();
        BoneCloths.Where(x => x).ForEach(GameObject.DestroyImmediate);
        BoneCloths.Clear();
    }

    public void HandleNewOutfit(Outfit outfit)
    {
        //Log.Info($"MagicaManager.HandleNewOutfit({outfit.DisplayName}");
        if (!targetPelvis) { Log.Warning("MagicaManager had no pelvis during HandleNewOutfit"); return; }

        var magiData = outfit.magiData;
        magiData.MeshCloths.ForEach(x => MeshClothSetup(x, outfit));
        magiData.BoneCloths.ForEach(x => BoneClothSetup(x, outfit));
        magiData.BoneSprings.ForEach(x => BoneSpringSetup(x, outfit));
    }

    void BoneClothSetup(MagicaCloth magica, Outfit outfit)
    {
        var liveMagica = GameObject.Instantiate(magica, targetPelvis.transform.parent);
        liveMagica.name = outfit.DisplayName + " BoneCloth";
        targetPelvis.DisableAnimator();
        liveMagica.ReplaceTransform(skeleton.GetAddBoneSet(outfit));
        liveMagica.SerializeData.colliderCollisionConstraint.colliderList.Clear();
        liveMagica.SerializeData.colliderCollisionConstraint.colliderList.AddRange(
            targetPelvis
            .MagiData
            .CapsuleColliders);
        liveMagica.SetParameterChange();
        processing.Add(liveMagica);
        var buildGuid = Guid.NewGuid();
        Log.Debug(buildGuid.ToString());
        liveMagica.OnBuildComplete += (x) => HandleBuildComplete(x, liveMagica, buildGuid);
        liveMagica.BuildAndRun();
        BoneCloths.Add(liveMagica);
        magica.gameObject.SetActive(true);
    }

    void MeshClothSetup(MagicaCloth magica, Outfit outfit)
    {
        magica
            .SerializeData
            .sourceRenderers
            .Where(x => 
                x.GetType() == typeof(SkinnedMeshRenderer))
            .Select(x =>
                new AccessoryDescriptor(
                    x as SkinnedMeshRenderer,
                    outfit.AssetName))
            .ToDictionary(
                x => x,
                x => magica)
            .ForEach(x => MeshClothAccs[x.Key] = x.Value);
    }

    void BoneSpringSetup(MagicaCloth magica, Outfit outfit)
    {
        Log.Warning($"{outfit.DisplayName} had BoneSpring component {magica.name}, but CarolCustomizer has not implemented any bonespring handling behavior");
    }

    public void HandleNewLiveAcc(LiveAccessory acc)
    {
        if (!MeshClothAccs.TryGetValue(acc.storedAcc, out var referenceMagica)) return;
        Log.Debug($"HandleNewLiveAcc({acc.Name})");

        if (LiveCloths.TryGetValue(acc, out var existing) && existing) GameObject.DestroyImmediate(existing.gameObject);

        if (!acc.isActive) return;

        targetPelvis.DisableAnimator();
        MeshClothAccs.Remove(acc.storedAcc);
        var boneDict = skeleton.GetAddBoneSet(acc.outfit);

        referenceMagica.gameObject.SetActive(false);

        var liveMagica = GameObject.Instantiate(referenceMagica, targetPelvis.transform.parent);
        liveMagica.SerializeData.cullingSettings.cameraCullingMode = CullingSettings.CameraCullingMode.Off;

        acc.AddToMagica(liveMagica);
        skeleton.AssignLiveBones(acc);
        liveMagica.SerializeData.colliderCollisionConstraint.colliderList.Clear();
        liveMagica.SerializeData.colliderCollisionConstraint.colliderList.AddRange(
            targetPelvis
            .MagiData
            .CapsuleColliders);
        liveMagica.name = acc.Name + " MeshCloth";
        liveMagica.ReplaceTransform(boneDict);
        liveMagica.SetParameterChange();
        var buildGuid = Guid.NewGuid();
        Log.Debug(buildGuid.ToString());
        liveMagica.OnBuildComplete += (x) => HandleBuildComplete(x, liveMagica, buildGuid);
        liveMagica.gameObject.SetActive(true);
        referenceMagica.gameObject.SetActive(true);
        LiveCloths[acc] = liveMagica;
    }

    void HandleBuildComplete(bool success, MagicaCloth component, Guid buildGuid)
    {
        processing.Remove(component);
        processing.RemoveAll(x => !x);
        //Log.Info($"HandleBuildComplete({buildGuid}): {success}.");
        if (!targetPelvis) { Log.Warning("build completed after pelvis was destroyed"); return; }

        if (processing.Any()) return;
        //Log.Info("enabling animator");
        targetPelvis.EnableAnimator();
    }
}
