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
    #endregion

    #region Public Interface
    public Transform[] bones => storedAcc.referenceSMR.bones;
    public string RootBoneName => storedAcc.referenceSMR.rootBone?.name ?? "CarolPelvis";

    public Outfit outfit => storedAcc?.outfit;

    public event Action OnAccessoryStateChanged;

    public bool isActive { get; private set; } = false;

    public LiveAccessory(StoredAccessory acc, Transform folder)
        : base(acc.Name, acc.Source)
    {
        this.folder = folder;
        storedAcc = acc;

        Materials = new MaterialDescriptor[acc.Materials.Length];
        acc.Materials
            .Select((mat, i) => (mat, i))
            .ForEach(tup => Materials[tup.i] = tup.mat);

        var liveObj = UnityEngine.Object.Instantiate(storedAcc.referenceSMR.gameObject, folder);
        if (!liveObj) { Log.Error($"Failed to instantiate {storedAcc.referenceSMR.name}."); return; }

        liveSMR = liveObj.GetComponent<SkinnedMeshRenderer>();
        if (!liveSMR) { Log.Error($"{storedAcc.referenceSMR.name} was instantiated without an SMR."); return; }

        liveSMR.allowOcclusionWhenDynamic = false;
        liveSMR.updateWhenOffscreen = true;
        liveObj.layer = Constants.SMRLayer;
    }

    public bool IsOnArmature(Transform pelvis)
    {
        if (!pelvis) return false;
        if (!liveSMR.rootBone) return false;
        return liveSMR.rootBone.IsChildOf(pelvis);
    }

    public void SetLiveBones(Transform[] liveBones, Transform rootBone)
    {
        if (!liveSMR) Log.Warning($"{this.Name} had a null SMR during SetLiveBones()");

        liveSMR.bones = liveBones;
        liveSMR.rootBone = rootBone;
    }

    virtual public void Enable()
    {
        isActive = true;
        liveSMR.gameObject.SetActive(true);
    }

    virtual public void Disable() 
    {
        isActive = false;
        liveSMR.gameObject.SetActive(false); 
    }

    public void AddToMagica(MagicaCloth magica)
    {
        GameObject.Destroy(liveSMR.gameObject);
        liveSMR = GameObject.Instantiate(storedAcc.referenceSMR, folder).GetComponent<SkinnedMeshRenderer>();
        liveSMR.allowOcclusionWhenDynamic = false;
        liveSMR.updateWhenOffscreen = true;
        ReapplyMaterials();
        magica.SerializeData.sourceRenderers.Clear();
        magica.SerializeData.sourceRenderers.Add(liveSMR);
        magica.SerializeData.rootBones.Clear();
        magica.SerializeData.rootBones.Add(liveSMR.transform);
        liveSMR.gameObject.layer = Constants.SMRLayer;
        liveSMR.gameObject.SetActive(isActive);
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
        Materials
            .Select((mat,index) => (mat,index))
            .ForEach((tup) => 
                liveSMR
                .ReplaceMaterialAtIndex(tup.mat.referenceMaterial, tup.index));
        //TODO: this is failing on faces?
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
