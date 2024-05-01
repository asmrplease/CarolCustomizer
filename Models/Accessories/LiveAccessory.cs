using System;
using UnityEngine;
using CarolCustomizer.Utils;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Behaviors.Carol;
using System.Linq;

namespace CarolCustomizer.Models.Accessories;
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

    virtual public void Enable() => liveSMR.gameObject.SetActive(true);

    virtual public void Disable() => liveSMR.gameObject.SetActive(false);

    public bool isActive => liveSMR.gameObject.activeSelf;


    public LiveAccessory(StoredAccessory acc, SkeletonManager skeleton, Transform folder)
        : base(acc.GetName(), acc.GetSource())
    {
        this.skeleton = skeleton;
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

    public void Refresh()
    {
        var liveBoneArray = skeleton.Mount(this);
        if (liveBoneArray is null) { Log.Error($"Failed to get live bones for {liveSMR.name}."); return; }
        if (liveBoneArray.Contains(null)) { Log.Warning($"Null bone was returned, {this.Name} may not appear as expected"); }

        liveSMR.bones = liveBoneArray;
        string rootBoneName = "CarolPelvis";
        if (storedAcc.referenceSMR.rootBone) { rootBoneName = storedAcc.referenceSMR.rootBone.name; }

        var newRoot = skeleton.GetLiveStandardBone(rootBoneName);
        if (!newRoot || newRoot is null) { Log.Warning($"Failed to find {this.Name}'s rootbone: {rootBoneName}"); }
        liveSMR.rootBone = newRoot;
    }

    public Transform[] GetReferenceBones()
    {
        return storedAcc.referenceSMR.bones;
    }

    public void SetLiveBones(Transform[] liveBones, Transform rootBone)
    {
        liveSMR.bones = liveBones;
        liveSMR.rootBone = rootBone;
    }

    public void Dispose()
    {
        if (!liveSMR) { Log.Warning("Tried to Dispose a LiveAcc with a null SMR."); return; }
        Log.Debug($"Disposing {liveSMR.name}...");

        Disable();

        UnityEngine.Object.Destroy(liveSMR);
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
