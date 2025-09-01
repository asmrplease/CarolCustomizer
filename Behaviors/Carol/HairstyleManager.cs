using CarolCustomizer.Events;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Utils;
using FuseBox.External.MagicaCloth2;
using MagicaCloth2;
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

    public HairstyleManager(CarolInstance player)
    {
        player.SpawnEvent += OnPelvisChanged;
    }

    public HairChangeEvent GetHairDescriptor()
    {
        string style = hairstyle is not null ?
            hairstyle.name : "Haircut_Powerhelmet";
        string color = hairMaterial is not null ?
            hairMaterial.name : "CRLH_Default_Brown";
        return new HairChangeEvent(style, color);
    }

    public void OnPelvisChanged(PelvisWatchdog newPelvis)
    {
        targetPelvis = newPelvis;
        InstantiateHairstyle();
        ApplyMaterial();
    }

    public void AssignHairstyle(Hairstyle hairstyle)
    {
        this.hairstyle = hairstyle;
        InstantiateHairstyle();
        ApplyMaterial();
    }

    public void AssignMaterial(Material material)
    {
        Log.Debug($"Assigning hair material {material.name}");
        if (!material) return;

        this.hairMaterial = material;
        ApplyMaterial();
    }

    void ApplyMaterial()
    {
        if (!hairMaterial) return;
        if (!liveHair) { Log.Error("No valid HairstyleManager.liveHair during HairstyleManager.ApplyMaterial()"); return; }

        var smr = liveHair.GetComponentInChildren<SkinnedMeshRenderer>(true);
        smr.sharedMaterial = hairMaterial;
    }

    void InstantiateHairstyle()
    {
        if (liveHair) GameObject.Destroy(liveHair);
        if (hairstyle is null) { Log.Warning("no valid hairstyle during HairstyleManager.InstantiateHairstyle"); return; }
        if (!targetPelvis) { Log.Error("No valid pelvis during HairstyleManager.InstantiateHairstyle()"); return; }

        var head = targetPelvis.BoneData.StandardBones["Bn_CarolHead"];
        if (!head) { Log.Error("No valid headbone when instantiating hairstyle"); return; }

        head.GetComponentsInChildren<Hairstyle>(true)
            .Select(x => x.gameObject)
            .ForEach(GameObject.Destroy);

        liveHair = GameObject.Instantiate(hairstyle.gameObject, head.transform);
        if (!liveHair) { Log.Error("Failed to instantiate liveHair during InstantiateHairstyle"); return; }

        Log.Debug("Instantiating Hairstyle");
        liveHair.transform.localScale = Vector3.one;
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
        UpdateColliders();
        Log.Info("InstantiateHairstyle() Complete.");
    }

    public void UpdateColliders()
    {
        Log.Debug("HairstyleManager.UpdateColliders()");
        if (!targetPelvis) { Log.Error("HairstyleManager.UpdateColliders() was invoked while the pelvis is null!"); return; }
        if (!targetPelvis.MagiData) { Log.Error("targetPelvis.MagiData was invalid."); return; }

        if (!targetPelvis.MagiData.ClothCompanion) 
        {
            targetPelvis.MagiData.ClothCompanion = targetPelvis.transform.parent.gameObject.AddComponent<MagicaClothCompanion>();
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


    public void Dispose() => GameObject.Destroy(liveHair);

}
