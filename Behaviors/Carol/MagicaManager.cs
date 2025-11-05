using CarolCustomizer.Contracts;
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
internal class MagicaManager
{
    Dictionary<LiveAccessory, MagicaCloth> LiveCloths = [];

    SkeletonManager skeleton;
    PelvisWatchdog targetPelvis;

    List<MagicaCloth> processing = [];
    Dictionary<AccessoryDescriptor, MagicaCloth> MeshClothAccs = [];
    List<MagicaCloth> BoneCloths = [];
    Outfit colliderSource;

    public string ColliderSourceName => colliderSource?.AssetName ?? Constants.Pyjamas;

    public MagicaManager(SkeletonManager skeletonManager)
    {
        this.skeleton = skeletonManager;
        //this.skeleton.OnBonesAdded += HandleNewOutfit;
        this.skeleton.OnLiveBonesAssigned += HandleNewLiveAcc;
    }

    public void HandleNewPelvis(PelvisWatchdog newPelvis)
    {
        Log.Debug("magicamanager.handleNewPelvis()");
        targetPelvis = newPelvis;
        if (colliderSource is not null) ApplyCollider();
        MeshClothAccs.Clear();
        BoneCloths.Where(x => x).ForEach(GameObject.Destroy);
        BoneCloths.Clear();
    }

    public void SetColliderSource(Outfit outfit)
    {
        if (outfit is null) { Log.Warning("outfit was null when setting collider source"); return; }

        colliderSource = outfit;
        ApplyCollider();
    }

    void ApplyCollider()
    {
        if (!targetPelvis || targetPelvis.Behavior is null) return;

        var sourceColliders = colliderSource
            .magiData
            .CapsuleColliders
            .Where(x => x)
            .ToDictionary(x => x.name);
        targetPelvis.MagiData
            .CapsuleColliders
            .Select(x =>
                (live: x
                , found: sourceColliders.TryGetValue(x.name, out var reference)
                , reference))
            .Where(tup => tup.found)
            .ForEach(tup => tup.live.CopyFrom(tup.reference));
    }

    public void SetupAccessoryMagica(LiveAccessory acc)
    {
        //Log.Info($"MagicaManager.HandleNewOutfit({outfit.DisplayName}");
        if (!targetPelvis) { Log.Warning("MagicaManager had no pelvis during HandleNewOutfit"); return; }

        var magiData = acc.magicaCloth;
        switch(magiData.SerializeData.clothType)
        {
            case ClothProcess.ClothType.MeshCloth: MeshClothSetup(magiData, acc); break;
            //case ClothProcess.ClothType.BoneCloth: BoneClothSetup(magiData, acc); break;
            case ClothProcess.ClothType.BoneSpring: BoneSpringSetup(magiData, acc); break;
            default: break;
        }
    }

    public void HandleSourceSetup(IAccessorySource source)
    {
        Log.Info($"HandleSourceSetup({source.Descriptor})");
        source.GetBoneCloths().ForEach(BoneClothSetup);
    }

    void BoneClothSetup(MagicaCloth magica)
    {
        Log.Debug($"BoneClothSetup({magica.name})");
        var liveMagica = GameObject.Instantiate(magica, targetPelvis.transform.parent);
        liveMagica.name = magica.name + " BoneCloth";
        targetPelvis.AnimData.DisableAnimator();
        //liveMagica.ReplaceTransform(skeleton.GetAddBoneSet(outfit));
        liveMagica.SerializeData.colliderCollisionConstraint.colliderList.Clear();
        liveMagica.SerializeData.colliderCollisionConstraint.colliderList.AddRange(
            targetPelvis
            .MagiData
            .CapsuleColliders);
        liveMagica.SetParameterChange();
        processing.Add(liveMagica);
        var buildGuid = Guid.NewGuid();
        Log.Debug(buildGuid.ToString());
        liveMagica.OnBuildComplete += (_, x) => HandleBuildComplete(x, liveMagica, buildGuid);
        liveMagica.BuildAndRun();
        BoneCloths.Add(liveMagica);
        magica.gameObject.SetActive(true);
    }

    void MeshClothSetup(MagicaCloth magica, LiveAccessory acc)
    {
        //magica
        //    .SerializeData
        //    .sourceRenderers
        //    .Where(x => 
        //        x.GetType() == typeof(SkinnedMeshRenderer))
        //    .Select(x => x )
        //    .ToDictionary(
        //        x => x,
        //        x => magica)
        //    .ForEach(x => MeshClothAccs[x.Key] = x.Value);
    }

    void BoneSpringSetup(MagicaCloth magica, LiveAccessory acc)
    {
        Log.Warning($"{acc.Name} had BoneSpring component {magica.name}, but CarolCustomizer has not implemented any bonespring handling behavior");
    }

    public void HandleNewLiveAcc(LiveAccessory acc)
    {
        //if (!MeshClothAccs.TryGetValue(acc.AsDescriptor(), out var referenceMagica)) return;
        if (!acc.magicaCloth) return;
        Log.Info($"MagicaManager.HandleNewLiveAcc({acc.Name})");

        SetupAccessoryMagica(acc);
        var referenceMagica = acc.magicaCloth;
        if (LiveCloths.TryGetValue(acc, out var existing) && existing) GameObject.Destroy(existing.gameObject);
        if (!acc.isActive) return;

        targetPelvis.AnimData.DisableAnimator();
        //MeshClothAccs.Remove(acc.AsDescriptor());
        var boneDict = skeleton.GetAddBoneSet(acc.Source, acc.BespokeBones);
        
        referenceMagica.gameObject.SetActive(false);
        var liveMagica = GameObject.Instantiate(referenceMagica, targetPelvis.transform.parent);
        liveMagica.SerializeData.cullingSettings.cameraCullingMode = CullingSettings.CameraCullingMode.Off;
        liveMagica.SerializeData.colliderCollisionConstraint.colliderList.Clear();
        liveMagica.SerializeData.sourceRenderers.Clear();
        liveMagica.SerializeData.rootBones.Clear();
        acc.AddToMagica(liveMagica);
        skeleton.AssignLiveBones(acc, false);
        liveMagica.SerializeData.colliderCollisionConstraint.colliderList.AddRange(
            targetPelvis
            .MagiData
            .CapsuleColliders);
        liveMagica.name = acc.Name + " MeshCloth";
        liveMagica.ReplaceTransform(boneDict);
        liveMagica.SetParameterChange();
        var buildGuid = Guid.NewGuid();
        Log.Debug(buildGuid.ToString());
        liveMagica.OnBuildComplete += (_, x) => HandleBuildComplete(x, liveMagica, buildGuid);
        liveMagica.BuildAndRun();
        liveMagica.gameObject.SetActive(true);
        referenceMagica.gameObject.SetActive(true);
        LiveCloths[acc] = liveMagica;
    }

    void HandleBuildComplete(bool _, MagicaCloth component, Guid __)
    {
        processing.Remove(component);
        processing.RemoveAll(x => !x);
        //Log.Info($"HandleBuildComplete({buildGuid}): {success}.");
        if (!targetPelvis) { Log.Warning("build completed after pelvis was destroyed"); return; }

        if (processing.Any()) return;
        //Log.Info("enabling animator");
        targetPelvis.AnimData.EnableAnimator();
    }
}
