using CarolCustomizer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CarolCustomizer.Behaviors;
using CarolCustomizer.Utils;

namespace CarolCustomizer.Models;
public class LiveAccessory : AccessoryDescriptor
{
    #region Dependencies
    protected readonly SkeletonManager skeleton;
    protected readonly Transform folder;
    protected readonly StoredAccessory storedAcc;
    #endregion

    #region Common Components
    protected SkinnedMeshRenderer liveSMR;
    #endregion

    #region Public Interface
    public Transform[] bones => storedAcc.referenceSMR.bones;
    public Outfit outfit => storedAcc.outfit;

    public Action OnAccessoryStateChanged;


    public LiveAccessory(StoredAccessory acc, SkeletonManager skeleton, Transform folder)
        : base(acc.GetName(), acc.GetSource())
    {
        this.skeleton = skeleton;
        this.folder = folder;
        this.storedAcc = acc;

        this.Materials = new MaterialDescriptor[acc.Materials.Length];
        int index = 0;
        foreach (var material in acc.Materials)
        {
            Materials[index] = acc.Materials[index];
            index++;
        }

        var liveObj = GameObject.Instantiate(storedAcc.referenceSMR.gameObject, folder);
        if (!liveObj) { Log.Error($"Failed to instantiate {storedAcc.referenceSMR.name}."); return; }

        liveSMR = liveObj.GetComponent<SkinnedMeshRenderer>();
        if (!liveSMR) { Log.Error($"{storedAcc.referenceSMR.name} was instantiated without an SMR."); return; }
    }

    virtual public void Refresh()
    {
        var liveBoneArray = skeleton.Mount(this);
        if (liveBoneArray is null) { Log.Error($"Failed to get live bones for {liveSMR.name}."); return; }

        liveSMR.bones = liveBoneArray;
        string rootBoneName;
        if (storedAcc.referenceSMR.rootBone) {rootBoneName = storedAcc.referenceSMR.rootBone.name; }
        else { rootBoneName = "CarolPelvis"; }

        
        liveSMR.rootBone = skeleton.GetLiveStandardBone(rootBoneName);
    }

    virtual public void Enable() => liveSMR.gameObject.SetActive(true);

    virtual public void Disable() => liveSMR.gameObject.SetActive(false);

    public bool isActive => liveSMR.gameObject.activeSelf;

    public void Dispose()
    {
        if (!liveSMR) { Log.Warning("Tried to Dispose a LiveAcc with a null SMR."); return; }
        Log.Debug($"Disposing {liveSMR.name}...");

        Disable();

        GameObject.Destroy(liveSMR);
        liveSMR = null;
        Log.Debug("Done.");
    }

    internal void ApplyMaterial(MaterialDescriptor material, int index)
    {
        Log.Debug($"ApplyMaterial({material.Name}, {index});");
        Materials[index] = material;
        liveSMR.ReplaceMaterialAtIndex(material.referenceMaterial, index);
        Log.Debug($"The material is now {liveSMR.materials[index]}.");
    }
    #endregion
}
