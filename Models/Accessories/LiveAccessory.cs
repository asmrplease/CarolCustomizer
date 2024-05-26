using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using MagicaCloth2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Models.Accessories;
public class LiveAccessory : AccessoryDescriptor
{
    #region Dependencies
    
    protected readonly Transform folder;
    public readonly StoredAccessory storedAcc;
    #endregion

    #region Common Components
    protected SkinnedMeshRenderer liveSMR;
    protected MagicaCloth liveMagica;
    #endregion

    #region Public Interface
    public Transform[] bones => storedAcc.referenceSMR.bones;
    public string RootBoneName => storedAcc.referenceSMR.rootBone?.name ?? "CarolPelvis";

    public Outfit outfit => storedAcc?.outfit;

    public Action OnAccessoryStateChanged;

    virtual public void Enable() => liveSMR.gameObject.SetActive(true);

    virtual public void Disable() => liveSMR.gameObject.SetActive(false);

    public bool isActive { get
        {
            if (liveSMR) return liveSMR.gameObject.activeSelf;
            return false;
        } }

    public LiveAccessory(StoredAccessory acc, SkeletonManager skeleton, Transform folder)
        : base(acc.Name, acc.Source)
    {
        //this.skeleton = skeleton;
        this.folder = folder;
        storedAcc = acc;

        Materials = new MaterialDescriptor[acc.Materials.Length];
        int index = 0;
        foreach (var material in acc.Materials)
        {
            Materials[index] = acc.Materials[index];
            index++;
        }

        var liveObj = UnityEngine.Object.Instantiate(storedAcc.referenceSMR.gameObject, folder);
        if (!liveObj) { Log.Error($"Failed to instantiate {storedAcc.referenceSMR.name}."); return; }

        liveSMR = liveObj.GetComponent<SkinnedMeshRenderer>();
        if (!liveSMR) { Log.Error($"{storedAcc.referenceSMR.name} was instantiated without an SMR."); return; }
    }

    //public void Refresh() => skeleton.AssignLiveBones(this);

    public void SetLiveBones(Transform[] liveBones, Transform rootBone)
    {
        liveSMR.bones = liveBones;
        liveSMR.rootBone = rootBone;
    }

    internal void ApplyMaterial(MaterialDescriptor material, int index)
    {
        Log.Debug($"ApplyMaterial({material.Name}, {index});");
        Materials[index] = material;
        liveSMR.ReplaceMaterialAtIndex(material.referenceMaterial, index);
        Log.Debug($"The material is now {liveSMR.materials[index]}.");
    }

    internal void ApplySharedMaterials(List<Material> materials)
    {
        liveSMR.SetSharedMaterials(materials);
    }

    public void CloneMagica(MagicaCloth referenceMagica, PelvisWatchdog pelvis, Dictionary<string, Transform> boneDict)
    {
        referenceMagica.gameObject.SetActive(false);
        if (liveMagica) GameObject.Destroy(liveMagica);

        liveMagica = GameObject.Instantiate(referenceMagica, pelvis.transform.parent);
        liveMagica.SerializeData.cullingSettings.cameraCullingMode = CullingSettings.CameraCullingMode.Off;

        liveMagica.SerializeData.sourceRenderers.Clear();
        liveMagica.SerializeData.sourceRenderers.Add(liveSMR);
        liveMagica.SerializeData.rootBones.Clear();
        liveMagica.SerializeData.rootBones.Add(liveSMR.transform);
        liveMagica.SerializeData.colliderCollisionConstraint.colliderList.Clear();
        liveMagica.SerializeData.colliderCollisionConstraint.colliderList.AddRange(
            pelvis
            .GetComponentsInChildren<MagicaCapsuleCollider>(true)
            .ToList());

        liveMagica.ReplaceTransform(boneDict);
        liveMagica.SetParameterChange();
        liveMagica.OnBuildComplete += (x) => HandleComplete(x, liveMagica, pelvis);
        liveMagica.gameObject.SetActive(true);
        referenceMagica.gameObject.SetActive(true);
    }

    void HandleComplete(bool success, MagicaCloth completed, PelvisWatchdog pelvis)
    {
        Log.Info($"{liveSMR.name}.BuildMagica({success})");
        if (pelvis) pelvis.CompData.Animator.enabled = true;

        Log.Info("Old magicas destroyed");
    }

    public void DestroyGameObject()
    {
        if (liveSMR) GameObject.Destroy(liveSMR.gameObject);
    }

    /*public void ApplyToMagica(MagicaCloth magica)
    {
        Log.Debug($"{liveSMR.name}.ApplyToMagica({magica.name})");
        var sdata = magica.SerializeData;
        if (sdata.clothType != ClothProcess.ClothType.MeshCloth) return;

        sdata.ReplaceTransform(new Dictionary<int, Transform> { { liveSMR.GetInstanceID(), liveSMR.transform } });
        magica.SetParameterChange();
    }*/


    public void BuildMagica(PelvisWatchdog pelvis)
    {
        Log.Info($"{liveSMR.name}.BuildCloth()");
        liveSMR
            .GetComponents<MagicaCloth>()
            .ToList()
            .ForEach(GameObject.Destroy);

        // var newMagica = GameObject
        //    .Instantiate(existing, liveSMR.transform)
        //    .GetComponent<MagicaCloth>();

        var newMagica = liveSMR
            .gameObject
            .AddComponent<MagicaCloth>();

        //var newMagica = pelvis
        //    .transform
        //    .parent
        //    .gameObject
        //    .AddComponent<MagicaCloth>();

        //newMagica.DisableAutoBuild();
        //newMagica.Initialize();

        var refMagica = outfit
            .compData
            .magicaCloths
            .First(x => x.SerializeData.clothType == ClothProcess.ClothType.MeshCloth);
            //.GetSerializeData2();
        var newSData = newMagica.SerializeData;
        newSData.Import(refMagica);

        newSData.clothType = ClothProcess.ClothType.MeshCloth;
        newSData.sourceRenderers.Clear();
        newSData.sourceRenderers.Add(liveSMR);
        newSData.reductionSetting.simpleDistance = 0.0212f;
        newSData.reductionSetting.shapeDistance = 0.0244f;
        newSData.paintMode = ClothSerializeData.PaintMode.Manual;//Texture_Fixed_Move;
        newSData.gravity = 1.0f;
        newSData.damping.SetValue(0.03f);
        newSData.angleRestorationConstraint.stiffness.SetValue(0.05f, 1.0f, 0.5f, true);
        newSData.angleRestorationConstraint.velocityAttenuation = 0.5f;
        newSData.angleLimitConstraint.useAngleLimit = true;
        newSData.angleLimitConstraint.limitAngle.SetValue(45.0f, 0.0f, 1.0f, true);
        newSData.distanceConstraint.stiffness.SetValue(0.5f, 1.0f, 0.5f, true);
        newSData.tetherConstraint.distanceCompression = 0.9f;
        newSData.inertiaConstraint.depthInertia = 0.7f;
        newSData.inertiaConstraint.movementSpeedLimit.SetValue(true, 3.0f);
        newSData.inertiaConstraint.particleSpeedLimit.SetValue(true, 3.0f);
        newSData.colliderCollisionConstraint.mode = ColliderCollisionConstraint.Mode.Point;

        newSData.cullingSettings.cameraCullingMode = CullingSettings.CameraCullingMode.Off;
        newSData.rootBones.Add(liveSMR.transform);
        newSData.colliderCollisionConstraint.colliderList.AddRange(
            pelvis
            .GetComponentsInChildren<MagicaCapsuleCollider>(true)
            .ToList());
        var refUsedTransform = new HashSet<Transform>();
        var newUsedTransform = new HashSet<Transform>();
        refMagica.SerializeData.GetUsedTransform(refUsedTransform);
        newSData.GetUsedTransform(newUsedTransform);
        //refSData.GetSerializeData2().GetUsedTransform(existing);
        refUsedTransform.ForEach(x => Log.Info($"new used transform {x}"));
        newUsedTransform.ForEach(x => Log.Info($"new used transform {x}"));            
        /*sdata.ReplaceTransform(
                liveSMR
                .bones
                .Where(x=>x)
                .ToDictionaryOverwrite(x => x.GetInstanceID()));*/
        //newMagica.SetParameterChange();
        //newMagica.OnBuildComplete += (x) => HandleComplete(x);
        //newMagica.BuildAndRun();
    }

    #endregion
}
