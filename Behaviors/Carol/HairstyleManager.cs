using CarolCustomizer.Events;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Utils;
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

        var smr = liveHair.GetComponentInChildren<SkinnedMeshRenderer>(true);
        smr.sharedMaterial = hairMaterial;
    }

    void InstantiateHairstyle()
    {
        if (liveHair) GameObject.Destroy(liveHair);
        var head = targetPelvis.BoneData.StandardBones["Bn_CarolHead"];
        if (!head) { Log.Error("No valid headbone when instantiating hairstyle"); return; }
        head.GetComponentsInChildren<Hairstyle>(true)
            .Select(x => x.gameObject)
            .ForEach(GameObject.Destroy);

        if (hairstyle is null) return;

        liveHair = GameObject.Instantiate(hairstyle.gameObject, head.transform);
        if (!liveHair) { Log.Error("Failed to instantiate liveHair during InstantiateHairstyle"); return; }

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
        //Magica activation
        UpdateColliders();
    }

    public void UpdateColliders()
    {
        if (!targetPelvis) { Log.Error("HairstyleManager.UpdateColliders() was invoked while the pelvis is null!"); return; }
        if (!targetPelvis.MagiData) { Log.Error("targetPelvis.MagiData was invalid."); return; }
        if (!targetPelvis.MagiData.ClothCompanion) { Log.Warning("No cloth companion found on carol."); return; }

        var hairMagica = liveHair.GetComponentInChildren<MagicaCloth>();
        if (!hairMagica) { Log.Warning($"no magica cloth found for {liveHair.name}."); return; }

        var hairCompanion = targetPelvis.MagiData.ClothCompanion;
        hairCompanion.cloth = hairMagica;
        if (hairCompanion.colliderRoots is null) { Log.Error("HairCompanion.ColliderRoots is null!"); return; }

        hairCompanion.colliderRoots.Clear();
        hairCompanion.colliderRoots.Add(targetPelvis.transform);
        hairCompanion.RebuildCollidersList();
    }


    public void Dispose() => GameObject.Destroy(liveHair);

}
