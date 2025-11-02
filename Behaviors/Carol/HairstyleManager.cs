using CarolCustomizer.Events;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Utils;
using FuseBox.External.MagicaCloth2;
using MagicaCloth2;
using Onirism.Gameplay;
using System;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Carol;
internal class HairstyleManager : IDisposable
{
    PelvisWatchdog targetPelvis;
    GameObject liveHair;
    public Hairstyle hairstyle { get; private set; }
    public Material hairMaterial { get; private set; }
    public event Action<HairChangeEvent> HairstyleChanged;

    public string HairstyleName => this.hairstyle ? this.hairstyle.name : "";
    public string HairMaterialName => hairMaterial ? hairMaterial.name : "";

    public HairChangeEvent GetHairDescriptor()
    {
        string style = hairstyle is not null ?
            hairstyle.name : "Haircut_Powerhelmet";
        string color = hairMaterial is not null ?
            hairMaterial.name : "CRLH_Default_Brown";
        return new HairChangeEvent(style, color);
    }

    public void HandleNewPelvis(PelvisWatchdog newPelvis)
    {
        targetPelvis = newPelvis;
        InstantiateHairstyle();
        ApplyMaterial();
    }

    public void AssignHairstyle(Hairstyle hairstyle)
    {
        if (hairstyle is null) { Log.Warning("Tried to apply a null hairstyle"); return; }

        Log.Debug($"Assigning hairstyle {hairstyle.name}");
        this.hairstyle = hairstyle;
        InstantiateHairstyle();
        ApplyMaterial();
        var e = GetHairDescriptor();
        HairstyleChanged?.Invoke(e);
    }

    public void AssignMaterial(Material material, bool dissolve = false)
    {
        if (material is null) { Log.Warning("Tried to apply a null hair material"); return; }

        if (dissolve)
        {
            ApplyDissolve(material);
            return;
        }

        Log.Debug($"Assigning hair material {material.name}");

        this.hairMaterial = material;
        ApplyMaterial();
        var e = GetHairDescriptor();
        HairstyleChanged?.Invoke(e);
    }

    public void ApplyDissolve(Material dissolve)
    {
        var smrs = liveHair.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var smr in smrs)
        {
            var mats = smr.materials.Select(x => dissolve).ToList();
            smr.SetSharedMaterials(mats);
        }   
    }

    void ApplyMaterial()
    {
        if (!hairMaterial) return;
        if (!liveHair) { Log.Error("No valid HairstyleManager.liveHair during HairstyleManager.ApplyMaterial()"); return; }

        var smr = liveHair.GetComponentInChildren<SkinnedMeshRenderer>(true);
        //smrs.ForEach(smr => smr.materials = smr.materials.Select(x => hairMaterial).ToArray());
        //smrs.ForEach(smr => smr.material = hairMaterial);
        //smr.material = hairMaterial;
        smr.ReplaceMaterialAtIndex(hairMaterial, hairstyle.mainMaterialIndex);
    }

    void InstantiateHairstyle2()
    {
        if (liveHair) GameObject.DestroyImmediate(liveHair);
        if (hairstyle is null) { Log.Warning("no valid hairstyle during HairstyleManager.InstantiateHairstyle"); return; }
        if (!targetPelvis) { Log.Error("No valid pelvis during HairstyleManager.InstantiateHairstyle()"); return; }

    }

    void InstantiateHairstyle()
    {
        if (liveHair) GameObject.DestroyImmediate(liveHair);
        if (hairstyle is null) { Log.Warning("no valid hairstyle during HairstyleManager.InstantiateHairstyle"); return; }
        if (!targetPelvis) { Log.Error("No valid pelvis during HairstyleManager.InstantiateHairstyle()"); return; }

        var head = targetPelvis.BoneData.StandardBones["Bn_CarolHead"];
        if (!head) { Log.Error("No valid headbone when instantiating hairstyle"); return; }

        var onModelHairstyles = head.GetComponentsInChildren<Hairstyle>(true);
        Log.Debug($"Found {onModelHairstyles.Count()} hairstyles during InstantiateHairstyle()");
        onModelHairstyles
            .Select(x => x.gameObject)
            .ForEach(GameObject.Destroy);
        Log.Debug($"Instantiating Hairstyle {hairstyle.name}");
        liveHair = GameObject.Instantiate(hairstyle.spawnPrefab, head.transform);
        if (!liveHair) { Log.Error("Failed to instantiate liveHair during InstantiateHairstyle"); return; }

        if (targetPelvis.gameObject.GetComponentInParent<Entity>() is Entity player)
        {
            player.inventory.currentHairstyle = liveHair;
        }
        liveHair.transform.localScale = Vector3.one;
        liveHair.transform.ResetLocalPosRot();
        liveHair.gameObject.SetActive(true);
        liveHair.GetComponentsInChildren<SkinnedMeshRenderer>(true)
            .ForEach(x => 
            { 
                x.gameObject.SetActive(true);
                x.gameObject.layer = Constants.SMRLayer;
                x.renderingLayerMask = Constants.SMRLayer;
                x.allowOcclusionWhenDynamic = false;
                x.updateWhenOffscreen = true;
            });
        Log.Info("InstantiateHairstyle() Complete.");
        UpdateColliders();
    }

    public void UpdateColliders()
    {
        Log.Debug("HairstyleManager.UpdateColliders()");
        if (!targetPelvis) { Log.Error("HairstyleManager.UpdateColliders() was invoked while the pelvis is null!"); return; }
        if (!targetPelvis.MagiData) { Log.Error("targetPelvis.MagiData was invalid."); return; }

        if (!targetPelvis.MagiData.ClothCompanion) 
        {
            if (targetPelvis.GetComponentInParent<MagicaClothCompanion>() is MagicaClothCompanion substitutue)
            {
                targetPelvis.MagiData.ClothCompanion = substitutue;
            }
            else targetPelvis.MagiData.ClothCompanion = targetPelvis.transform.parent.gameObject.AddComponent<MagicaClothCompanion>();
        }
        Log.Debug("targetPelvis.MagiData.ClothCompanion is not null.");

        if (!liveHair) { Log.Error("HairstyleManager.liveHair is null"); return; }
        Log.Debug("HairstyleManager.liveHair is valid");

        var hairMagica = liveHair.GetComponentInChildren<MagicaCloth>();
        if (!hairMagica) { Log.Warning($"no magica cloth found for liveHair."); return; }
        Log.Debug("live hair magica is valid.");

        var hairCompanion = targetPelvis.MagiData.ClothCompanion;
        hairCompanion.cloth = hairMagica;
        hairCompanion.colliderRoots ??= [];
        hairCompanion.colliderRoots.Clear();
        hairCompanion.colliderRoots.Add(targetPelvis.transform);
        hairCompanion.RebuildCollidersList();
    }

    public void Dispose() => GameObject.Destroy(liveHair.gameObject);

}
