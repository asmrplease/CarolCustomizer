using CarolCustomizer.Assets;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Utils;
using FuseBox.External.MagicaCloth2;
using MagicaCloth2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static System.Linq.Enumerable;

namespace CarolCustomizer.Models.Outfits;
public class CompData : MonoBehaviour
{
    static readonly HashSet<Type> SkipTypes =
    [
         typeof(Transform)
        ,typeof(PelvisWatchdog)
        ,typeof(BoneData)
        ,typeof(CompData)
        ,typeof(DynamicBone)
        ,typeof(MagicaCapsuleCollider)
        ,typeof(MagicaClothCompanion)
        ,typeof(MagicaCloth)
        ,typeof(SkinnedMeshRenderer)
    ];

    [SerializeField]
    public List<SkinnedMeshRenderer> allSMRs;

    public List<GameObject> EffectGameObjects { get; private set; }
    public List<Behaviour> EffectBehaviours { get; private set; }

    public SkinnedMeshRenderer BaseFace => allSMRs.FirstOrDefault(x => x && x.name == "tete");

    public CoopModelToggle[] coopToggles { get; private set; }

    public List<SkinnedMeshRenderer>[] coopMeshes
        { get; private set; }
        = new List<SkinnedMeshRenderer>[Constants.MaxCoopPlayers];
    public List<SkinnedMeshRenderer> allCoopMeshes{ get; private set; } = [];


    public CompData Constructor()
    {
        allSMRs = transform.parent
            .GetComponentsInChildren<SkinnedMeshRenderer>(true)
            .ToList();
        coopToggles = transform.parent.GetComponentsInChildren<CoopModelToggle>(true);
        SetCoopVariants();
        coopToggles.ForEach(x => x.enabled = false);
        EffectSetup();
        return this;
    }

    void EffectSetup()
    {
        var allComponents = GetComponentsInChildren<Component>(true);

        GetComponentsInChildren<Animator>(true)
            .ForEach(x =>
                x.cullingMode = AnimatorCullingMode.AlwaysAnimate);

        var effectComponents = allComponents.Where(x => !SkipTypes.Contains(x.GetType()));

        EffectBehaviours = 
            effectComponents
            .Where(x => x
                .GetType()
                .IsAssignableFrom(typeof(Behaviour)))
            .Select(x => x as Behaviour)
            .ToList();
        var nonBehaviors = 
            effectComponents
            .Except(EffectBehaviours)
            .Select(x => x.transform)
            .Where(x => !CommonBones.IsCommon(x.name));
        EffectGameObjects = nonBehaviors
            .Where(x => !nonBehaviors.Contains(x.parent))
            .Select(x => x.gameObject)
            .ToList();
        EffectBehaviours.ForEach(x => x.enabled = false);
        EffectGameObjects.ForEach(x => x.SetActive(false));
    }

    void SetCoopVariants()
    {
        foreach (int i in Range(0, Constants.MaxCoopPlayers))
        {
            var playerToggles = coopToggles.Where(x=>x.playerNumberToggle == i);
            coopMeshes[i] = [];
            if (playerToggles is null) continue;

            playerToggles
                .Select(x => x.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                .ForEach(coopMeshes[i].AddRange)
                .ForEach(allCoopMeshes.AddRange);
        }
    }
    public void SetBaseVisibility(bool visible)
    {
        Log.Debug("CompData.SetBaseVisibility()");
        if (allSMRs is null) return;

        allSMRs
            .Where(x => x && !x.transform.IsChildOf(this.transform))
            .ForEach(x => x.gameObject.SetActive(visible));
    }

    void OnDestory()
    {
        coopToggles.ForEach(x => x.enabled = true);
    }
}
