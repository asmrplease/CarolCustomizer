using CarolCustomizer.Events;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Utils;
using System;
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
        if (hairstyle is null) return;

        var head = targetPelvis.BoneData.StandardBones["Bn_CarolHead"];
        if (!head) { Log.Error("No valid headbone when instantiating hairstyle"); return; }

        liveHair = GameObject.Instantiate(hairstyle.gameObject, head.transform);
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
        //Magica activation?
    }

    public void Dispose() => GameObject.Destroy(liveHair);

}
