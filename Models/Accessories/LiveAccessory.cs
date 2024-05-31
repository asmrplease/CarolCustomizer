﻿using CarolCustomizer.Models.Outfits;
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
    #endregion

    #region Public Interface
    public Transform[] bones => storedAcc.referenceSMR.bones;
    public string RootBoneName => storedAcc.referenceSMR.rootBone?.name ?? "CarolPelvis";

    public Outfit outfit => storedAcc?.outfit;

    public Action OnAccessoryStateChanged;

    public SkinnedMeshRenderer DEBUG_GET_SMR() => liveSMR;

    virtual public void Enable() => liveSMR.gameObject.SetActive(true);

    virtual public void Disable() => liveSMR.gameObject.SetActive(false);

    public bool isActive => liveSMR?.gameObject.activeSelf ?? false;

    public LiveAccessory(StoredAccessory acc, Transform folder)
        : base(acc.Name, acc.Source)
    {
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

    public void SetLiveBones(Transform[] liveBones, Transform rootBone)
    {
        if (!liveSMR) Log.Warning($"{this.Name} had a null SMR during SetLiveBones()");
        liveSMR.bones = liveBones;
        liveSMR.rootBone = rootBone;
    }

    public void AddToMagica(MagicaCloth magica)
    {
        GameObject.Destroy(liveSMR.gameObject);
        liveSMR = GameObject.Instantiate(storedAcc.referenceSMR, folder).GetComponent<SkinnedMeshRenderer>();
        ReapplyMaterials();
        magica.SerializeData.sourceRenderers.Clear();
        magica.SerializeData.sourceRenderers.Add(liveSMR);
        magica.SerializeData.rootBones.Clear();
        magica.SerializeData.rootBones.Add(liveSMR.transform);
        if (liveSMR.bones.Count() != storedAcc.referenceSMR.bones.Count() + 2)
        {
            Log.Warning("smr bone count was short");
            var update = liveSMR.bones.ToList();
            update.Add(liveSMR.rootBone);
            update.Add(liveSMR.transform);
            liveSMR.bones = update.ToArray();
        }
    }

    internal void ApplyMaterial(MaterialDescriptor material, int index)
    {
        Log.Debug($"ApplyMaterial({material.Name}, {index});");
        Materials[index] = material;
        liveSMR.ReplaceMaterialAtIndex(material.referenceMaterial, index);
        Log.Debug($"The material is now {liveSMR.materials[index]}.");
    }

    void ReapplyMaterials()
    {
        var i = 0;
        Materials.ForEach((x) => liveSMR.ReplaceMaterialAtIndex(x.referenceMaterial, i));
    }

    internal void ApplySharedMaterials(List<Material> materials)
    {
        liveSMR.SetSharedMaterials(materials);
    }

    public void DestroyGameObject()
    {
        if (liveSMR) GameObject.Destroy(liveSMR.gameObject);
    }
    #endregion
}
