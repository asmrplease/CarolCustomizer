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
    static readonly HashSet<Type> SkipTypes = new() 
    {
         typeof(Transform)
        ,typeof(PelvisWatchdog)
        ,typeof(BoneData)
        ,typeof(CompData)
        ,typeof(DynamicBone)
        ,typeof(MagicaCapsuleCollider)
        ,typeof(MagicaClothCompanion)
        ,typeof(MagicaCloth)
        ,typeof(SkinnedMeshRenderer)
    };


    [SerializeField]
    RuntimeAnimatorController controller;
    public RuntimeAnimatorController Controller => controller;

    [SerializeField]
    Animator animator;
    public Animator Animator => animator;

    [SerializeField]
    public List<SkinnedMeshRenderer> allSMRs;

    public List<GameObject> EffectGameObjects { get; private set; }
    public List<Behaviour> EffectBehaviours { get; private set; }

    public SkinnedMeshRenderer BaseFace => allSMRs.FirstOrDefault(x => x && x.name == "tete");

    Dictionary<Type, Component> parentComponents = new();
    public CoopModelToggle[] coopToggles { get; private set; }

    public List<SkinnedMeshRenderer>[] coopMeshes
        { get; private set; }
        = new List<SkinnedMeshRenderer>[Constants.MaxCoopPlayers];
    public List<SkinnedMeshRenderer> allCoopMeshes
        { get; private set; }
        = new();

    public CompData Constructor()
    {
        allSMRs = transform
            .parent
            .GetComponentsInChildren<SkinnedMeshRenderer>(true)
            .ToList();

        animator ??= GetComponentsInParent<Animator>(true)?
            .FirstOrDefault(x => x.runtimeAnimatorController);

        controller ??= animator?.runtimeAnimatorController;
        coopToggles = transform.parent.GetComponentsInChildren<CoopModelToggle>(true);

        SetCoopVariants();
        coopToggles.ForEach(x => x.enabled = false);
        RefreshParentComponents();
        EffectSetup();
        return this;
    }

    void EffectSetup()
    {
        var allComponents = GetComponentsInChildren<Component>(true);

        GetComponentsInChildren<Animator>(true)
            .ForEach(x =>
                x.cullingMode = AnimatorCullingMode.AlwaysAnimate);

        var effectComponents = 
            allComponents
            .Where(x => 
                !SkipTypes
                .Contains(x.GetType()));

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


        EffectGameObjects = 
            nonBehaviors
            .Where(x =>
                !nonBehaviors
                .Contains(x.parent))
            .Select(x => x.gameObject)
            .ToList();
        
        foreach (var behaviour in EffectBehaviours)
        {
            behaviour.enabled = false;
        }

        foreach (var go in EffectGameObjects)
        {
            go.SetActive(false);
        }
    }

    void SetCoopVariants()
    {
        foreach (int i in Range(0, Constants.MaxCoopPlayers))
        {
            var playerToggles = coopToggles.Where(x=>x.playerNumberToggle == i);
            coopMeshes[i] = new();
            if (playerToggles is null) continue;
            foreach (var toggle in playerToggles)
            {
                var smrs = toggle.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                coopMeshes[i].AddRange(smrs);
                allCoopMeshes.AddRange(smrs);
            }
        }
    }

    public Component GetParentComponent(Type type)
    {
        parentComponents.TryGetValue(type, out Component component);
        return component;
    }

    public void RefreshParentComponents()
    {
        var components = GetComponentsInParent<Component>(true);
        if (components is null || !components.Any()) { parentComponents = new(); return; }

        parentComponents = components
            .Where(x => x is not null)
            .ToDictionaryOverwrite(x => x.GetType());
    }

    void OnDestory()
    {
        coopToggles.ForEach(x => x.enabled = true);
    }
}
