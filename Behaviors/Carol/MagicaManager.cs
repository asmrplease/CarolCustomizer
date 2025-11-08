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
    SkeletonManager skeleton;
    PelvisWatchdog targetPelvis;

    List<MagicaCloth> processing = [];
    Dictionary<AccessoryDescriptor, MagicaCloth> MeshCloths = [];
    Dictionary<int, MagicaCloth> BoneCloths = [];
    IAccessorySource colliderSource;

    public SourceDescriptor ColliderSourceName => colliderSource.Descriptor ?? Constants.PyjamaDescriptor;

    public MagicaManager(SkeletonManager skeletonManager)
    {
        this.skeleton = skeletonManager;
        this.skeleton.OnLiveBonesAssigned += AccMeshClothSetup;
    }

    public void HandleNewPelvis(PelvisWatchdog newPelvis)
    {
        Log.Debug("magicamanager.handleNewPelvis()");
        targetPelvis = newPelvis;
        if (colliderSource is not null) ApplyCollider();
        MeshCloths
            .Select(kvp => kvp.Value)
            .Where(x => x)
            .ForEach(GameObject.DestroyImmediate);
        MeshCloths.Clear();
        BoneCloths
            .Select(kvp => kvp.Value)
            .Where(x => x)
            .ForEach(GameObject.DestroyImmediate);
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

    void BoneClothSetup(MagicaCloth refMagica, IAccessorySource source)
    {
        if (BoneCloths.TryGetValue(refMagica.GetInstanceID(), out var existing) && existing) { GameObject.Destroy(existing); }

        var liveMagica = GameObject.Instantiate(refMagica, targetPelvis.transform.parent);
        liveMagica.name = refMagica.name + " BoneCloth";
        CommonSetup(liveMagica, source.Descriptor, source.GetBespokeBones());
        BoneCloths[refMagica.GetInstanceID()] = liveMagica;
    }

    public void AccMeshClothSetup(LiveAccessory acc)
    {
        if (!acc.meshCloth) return;

        Log.Info($"MagicaManager.HandleNewLiveAcc({acc.Name})");
        if (MeshCloths.TryGetValue(acc, out var existing) && existing) GameObject.Destroy(existing);
        if (!acc.isActive) return;

        var referenceMagica = acc.meshCloth;
        referenceMagica.gameObject.SetActive(false);
        var liveMagica = GameObject.Instantiate(referenceMagica, targetPelvis.transform.parent);
        liveMagica.SerializeData.sourceRenderers.Clear();
        liveMagica.SerializeData.rootBones.Clear();
        acc.AddToMagica(liveMagica);
        skeleton.AssignLiveBones(acc, false);
        CommonSetup(liveMagica, acc.Source, acc.BespokeBones);
        liveMagica.name = acc.Name + " MeshCloth";
        referenceMagica.gameObject.SetActive(true);
        MeshCloths[acc] = liveMagica;
    }

    void CommonSetup(MagicaCloth liveMagica, SourceDescriptor source, List<Transform> bespokeBones)
    {
        targetPelvis.AnimData.DisableAnimator();
        var boneDict = skeleton.GetAddBoneSet(source, bespokeBones);
        liveMagica.ReplaceTransform(boneDict);
        liveMagica.SerializeData.cullingSettings.cameraCullingMode = CullingSettings.CameraCullingMode.Off;
        liveMagica.SerializeData.colliderCollisionConstraint.colliderList.Clear();
        liveMagica.SerializeData.colliderCollisionConstraint.colliderList.AddRange(
            targetPelvis
            .MagiData
            .CapsuleColliders);
        liveMagica.SetParameterChange();
        var buildGuid = Guid.NewGuid();
        Log.Debug($"Starting build {buildGuid.ToString()} for {source}");
        liveMagica.OnBuildComplete += (_, x) => HandleBuildComplete(x, liveMagica, buildGuid);
        liveMagica.BuildAndRun();
        processing.Add(liveMagica);
        liveMagica.gameObject.SetActive(true);
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
