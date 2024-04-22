using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Models.Outfits;
public class CompData : MonoBehaviour
{
    [SerializeField]
    RuntimeAnimatorController controller;
    public RuntimeAnimatorController Controller => controller;

    [SerializeField]
    Animator animator;
    public Animator Animator => animator;

    [SerializeField]
    public List<SkinnedMeshRenderer> baseMeshes;

    public SkinnedMeshRenderer BaseFace => baseMeshes.FirstOrDefault(x => x.name == "tete");

    Dictionary<Type, Component> parentComponents = new();
    public CoopModelToggle[] coopToggles { get; private set; }

    //foreach coop player, list the SMRs associated with that player

    public CompData Constructor()
    {
        baseMeshes = transform
            .parent
            .GetComponentsInChildren<SkinnedMeshRenderer>(true)
            .ToList();

        animator ??= GetComponentsInParent<Animator>(true)?
            .FirstOrDefault(x => x.runtimeAnimatorController);
        if (!animator) Log.Warning("no animator found during CD.Constructor()");
        controller ??= animator?.runtimeAnimatorController;
        if (!controller) Log.Warning("no RAC found during CD.Constructor()");

        coopToggles = transform.parent.GetComponentsInChildren<CoopModelToggle>(true);
        Log.Debug($"{coopToggles.Count()} cooptoggles found.");
        foreach (var toggle in coopToggles) {toggle.enabled = false;}
        RefreshParentComponents();
        return this;
    }

    public Component GetParentComponent(Type type)
    {
        parentComponents.TryGetValue(type, out Component component);
        return component;
    }

    public void RefreshParentComponents()
    {
        parentComponents = GetComponentsInParent<Component>(true)
            .ToDictionaryOverwrite(x => x.GetType());
    }

    //public void OnTransformParentChanged() => Constructor();

    public void SetRAC(RuntimeAnimatorController controller)
    {
        if (!animator) { Log.Warning("No animator component"); return; }
        if (!controller) { Log.Warning("tried to set RAC as null"); return; }

        animator.runtimeAnimatorController = controller;
    }

    void OnDestory()
    {
        foreach (var toggle in coopToggles) toggle.enabled = true;
    }
}
