using CarolCustomizer.Assets;
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
    IAccessorySource colliderSource;

    public SourceDescriptor ColliderSourceName => colliderSource.Descriptor ?? Constants.PyjamaDescriptor;

    public MagicaManager(SkeletonManager skeletonManager)
    {
        this.skeleton = skeletonManager;
        this.skeleton.OnLiveBonesAssigned += HandleNewLiveAcc;
    }

    public void HandleNewPelvis(PelvisWatchdog newPelvis)
    {
        Log.Debug("magicamanager.handleNewPelvis()");
        targetPelvis = newPelvis;
        if (colliderSource is not null) ApplyCollider();
        MeshClothAccs.Clear();
        BoneCloths.Where(x => x).ForEach(GameObject.DestroyImmediate);
        BoneCloths.Clear();
    }

    public void SetColliderSource(SourceDescriptor descriptor)
    {
        if (descriptor is null) { Log.Warning("SetColliderSource was passed a null source descriptor"); return; }

        if (OutfitAssetManager.GetAccessorySource(descriptor) is not IAccessorySource source) return;
        if (source.GetColliders() is null) { Log.Warning($"ColliderSource {descriptor} was valid but has no colliders"); return; }
        colliderSource = source;

        ApplyCollider();
    }

    void ApplyCollider()
    {
        if (!targetPelvis || targetPelvis.Behavior is null) return;

        var sourceColliders = colliderSource
            .GetColliders()
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

    public void HandleSourceSetup(IAccessorySource source)
    {
        Log.Info($"HandleSourceSetup({source.Descriptor})");
        source.GetBoneCloths()
            .ForEach(x => BoneClothSetup(x, source));
    }

    void BoneClothSetup(MagicaCloth magica, IAccessorySource source)
    {
        Log.Debug($"BoneClothSetup({magica.name})");
        var liveMagica = GameObject.Instantiate(magica, targetPelvis.transform.parent);
        liveMagica.name = magica.name + " BoneCloth";
        targetPelvis.AnimData.DisableAnimator();
        //TODO: can we refactor this?
        liveMagica.ReplaceTransform(skeleton.GetAddBoneSet(source.Descriptor, source.GetBespokeBones()));
        liveMagica.SerializeData.cullingSettings.cameraCullingMode = CullingSettings.CameraCullingMode.Off;
        liveMagica.SerializeData.colliderCollisionConstraint.colliderList.Clear();
        liveMagica.SerializeData.colliderCollisionConstraint.colliderList.AddRange(
            targetPelvis
            .MagiData
            .CapsuleColliders);
        liveMagica.SetParameterChange();
        processing.Add(liveMagica);
        var buildGuid = Guid.NewGuid();
        Log.Debug($"Starting build {buildGuid.ToString()} for {source}");
        liveMagica.OnBuildComplete += (_, x) => HandleBuildComplete(x, liveMagica, buildGuid);
        liveMagica.BuildAndRun();
        BoneCloths.Add(liveMagica);
        magica.gameObject.SetActive(true);
    }

    public void HandleNewLiveAcc(LiveAccessory acc)
    {
        //if (!MeshClothAccs.TryGetValue(acc.AsDescriptor(), out var referenceMagica)) return;
        if (!acc.meshCloth) return;
        Log.Info($"MagicaManager.HandleNewLiveAcc({acc.Name})");

        var referenceMagica = acc.meshCloth;
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
        Log.Debug($"Starting build {buildGuid.ToString()} for {acc.Name}");
        liveMagica.OnBuildComplete += (_, x) => HandleBuildComplete(x, liveMagica, buildGuid);
        liveMagica.BuildAndRun();
        liveMagica.gameObject.SetActive(true);
        referenceMagica.gameObject.SetActive(true);
        LiveCloths[acc] = liveMagica;
    }

    void HandleBuildComplete(bool success, MagicaCloth component, Guid buildGuid)
    {
        processing.Remove(component);
        processing.RemoveAll(x => !x);
        Log.Info($"HandleBuildComplete({success}, {component}, {buildGuid}.");
        if (!targetPelvis) { Log.Warning("build completed after pelvis was destroyed"); return; }

        if (processing.Any()) return;

        targetPelvis.AnimData.EnableAnimator();
    }
}
