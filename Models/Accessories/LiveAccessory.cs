using CarolCustomizer.Models.Materials;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using MagicaCloth2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Models.Accessories;
public class LiveAccessory : AccessoryDescriptor, IDisposable
{
    #region Dependencies
    Transform folder;
    public readonly StoredAccessory storedAcc;
    #endregion

    #region Common Components
    protected SkinnedMeshRenderer liveSMR;
    #endregion

    #region Public Interface
    public Transform[] bones => storedAcc.referenceSMR.bones;
    public string RootBoneName => storedAcc.referenceSMR.rootBone?.name ?? "CarolPelvis";

    public Outfit outfit => storedAcc?.outfit;

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
        Reinstantiate();
    }

    void Reinstantiate()
    {
        if (liveSMR) GameObject.Destroy(liveSMR.gameObject);

        var liveObj = UnityEngine.Object.Instantiate(storedAcc.referenceSMR.gameObject, folder);
        if (!liveObj) { Log.Error($"Failed to instantiate {storedAcc.referenceSMR.name}."); return; }

        liveSMR = liveObj.GetComponent<SkinnedMeshRenderer>();
        if (!liveSMR) { Log.Error($"{storedAcc.referenceSMR.name} was instantiated without an SMR."); return; }

        liveSMR.allowOcclusionWhenDynamic = false;
        liveSMR.updateWhenOffscreen = true;
        liveObj.layer = Constants.SMRLayer;
        Materials
            .Select((mat, index) => (mat, index))
            .ForEach((tup) => liveSMR.ReplaceMaterialAtIndex(tup.mat.referenceMaterial, tup.index));
        liveSMR.gameObject.SetActive(isActive);
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

    public void SetVisible(bool visible)
    {
        if (!liveSMR) return;
        if (!liveSMR.gameObject) return;

        liveSMR.enabled = visible;
    }

    public void AddToMagica(MagicaCloth magica)
    {
        Reinstantiate();
        magica.SerializeData.sourceRenderers.Add(liveSMR);
        magica.SerializeData.rootBones.Add(liveSMR.transform);
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

    public void Dispose()
    {
        if (liveSMR) GameObject.Destroy(liveSMR.gameObject);
    }
    #endregion
}
