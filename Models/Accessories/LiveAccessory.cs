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
    public string RootBoneName => storedAcc.referenceSMR.rootBone?
        storedAcc.referenceSMR.rootBone.name
        :"CarolPelvis";
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

    public void Refresh() => skeleton.AssignLiveBones(this);

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

    public void DestroyGameObject()
    {
        if (liveSMR) GameObject.Destroy(liveSMR.gameObject);
    }
    #endregion
}
