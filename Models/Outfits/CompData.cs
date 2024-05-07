using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static System.Linq.Enumerable;

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
    public List<SkinnedMeshRenderer> allSMRs;

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
        //if (!animator) Log.Warning("no animator found during CD.Constructor()");
        controller ??= animator?.runtimeAnimatorController;
        //if (!controller) Log.Warning("no RAC found during CD.Constructor()");

        coopToggles = transform.parent.GetComponentsInChildren<CoopModelToggle>(true);
        //Log.Debug($"{coopToggles.Count()} cooptoggles found.");
        SetCoopVariants();
        coopToggles.ForEach(x => x.enabled = false);
        RefreshParentComponents();
        return this;
    }

    private void SetCoopVariants()
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
        parentComponents = GetComponentsInParent<Component>(true)
            .ToDictionaryOverwrite(x => x.GetType());
    }

    public void SetRAC(RuntimeAnimatorController controller)
    {
        if (!animator) { Log.Warning("No animator component"); return; }
        if (!controller) { Log.Warning("tried to set RAC as null"); return; }

        animator.runtimeAnimatorController = controller;
    }

    public int CountOnPelvis<T>() where T : Component
    {
        return this.transform.GetComponentsInChildren<T>().Count();
    }

    void OnDestory()
    {
        foreach (var toggle in coopToggles) toggle.enabled = true;
    }
}
