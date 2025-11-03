using CarolCustomizer.Behaviors.Carol;
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
    readonly SkinnedMeshRenderer referenceSMR;
    #endregion

    #region Common Components
    protected SkinnedMeshRenderer liveSMR;
    #endregion

    #region Public Interface
    public Transform[] bones => referenceSMR.bones;
    public string RootBoneName => referenceSMR.rootBone?.name ?? "CarolPelvis";

    public readonly Outfit outfit;

    public readonly List<Transform> BespokeBones;

    public readonly MagicaCloth magicaCloth;

    public bool isActive { get; private set; } = false;

    public LiveAccessory(StoredAccessory acc, Transform folder, MagicaCloth magica = null)
        : base(acc.referenceSMR, acc.Source)
    {
        this.folder = folder;
        this.referenceSMR = acc.referenceSMR;
        this.outfit = acc.outfit;
        this.BespokeBones = outfit.boneData.BespokeBones;
        this.Materials = new MaterialDescriptor[acc.Materials.Length];
        this.magicaCloth = magica;
        acc.Materials
            .Select((mat, i) => (mat, i))
            .ForEach(tup => Materials[tup.i] = tup.mat);
        Reinstantiate();
    }

    public LiveAccessory(StoredHair hair, Transform folder)
        :base (hair.smr, hair.Source)
    {
        this.folder = folder;
        this.referenceSMR = hair.smr;
        this.outfit = null;
        this.BespokeBones = hair.BespokeBones;
        this.referenceSMR = hair.smr;
        this.Materials = new MaterialDescriptor[hair.smr.sharedMaterials.Length];
        this.magicaCloth = hair.hairstyle.cloth;
        hair.smr.sharedMaterials
            .Select((mat, i) => (mat, i))
            .ForEach(tup => Materials[tup.i] = new MaterialDescriptor(tup.mat, hair.Source));
        Reinstantiate();
    }

    void Reinstantiate()
    {
        Log.Debug($"LiveAccessory {this.Name}.Reinstantiate()");
        if (liveSMR) GameObject.Destroy(liveSMR.gameObject);

        var liveObj = UnityEngine.Object.Instantiate(referenceSMR.gameObject, folder);
        if (!liveObj) { Log.Error($"Failed to instantiate {referenceSMR.name}."); return; }

        liveSMR = liveObj.GetComponent<SkinnedMeshRenderer>();
        if (!liveSMR) { Log.Error($"{referenceSMR.name} was instantiated without an SMR."); return; }

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

    public AccessoryDescriptor AsDescriptor()
    {
        return new AccessoryDescriptor(this);
    }

    public void Dispose()
    {
        if (liveSMR) GameObject.Destroy(liveSMR.gameObject);
    }
    #endregion
}
