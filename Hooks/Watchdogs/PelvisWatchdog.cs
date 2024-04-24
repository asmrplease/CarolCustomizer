using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using Slate;
using CarolCustomizer.Assets;
using MonoMod.Utils;
using CarolCustomizer.Models.Outfits;

namespace CarolCustomizer.Hooks.Watchdogs;
public class PelvisWatchdog : MonoBehaviour
{   
    readonly Guid Guid = Guid.NewGuid();
    protected Guid SourceGuid;

    [SerializeField]
    BoneData boneData;
    public BoneData BoneData { get { return boneData; } }

    [SerializeField]
    CompData compData;
    public CompData CompData { get { return compData; } }

    string parentName => transform.parent?.name ?? "none";
    string grandparentName => transform.parent?.parent?.name ?? "none";
    string rootName => transform.root?.name ?? "none";

    List<(Func<Predicate<PelvisWatchdog>, bool>,
    Predicate<PelvisWatchdog>)> checks;

    virtual public PelvisWatchdog BuildFromExisting(PelvisWatchdog watchdog, Component typeComponent)
    {
        Log.Debug("Watchdog.CopyFromExisting");
        if (!watchdog.BoneData || !watchdog.CompData) Log.Warning($"{watchdog} was missing data when constructing {this}.");
        SourceGuid = watchdog.Guid;
        boneData = watchdog.BoneData;
        compData = watchdog.CompData;
        return this;
    }

    virtual public void Awake()
    {
        Log.Debug($"{this}.Awake()");
        if (!boneData) boneData = this.gameObject.AddComponent<BoneData>().Constructor();
        if (!compData) compData = this.gameObject.AddComponent<CompData>().Constructor();
        DetectType();
    }

    virtual protected void OnTransformParentChanged() => DetectType(); 

    void SetupCheckList()
    {
        checks = new()
        {
            (Check<VirtualCarol,    MPBotWatchdog>,  (x)=> true),
            (Check<Entity,          PlayerWatchdog>, (x)=> x.rootName == "CAROL(Clone)"),
            (Check<Entity,          BotWatchdog>,    (x)=> true), 
            (Check<CutsceneActor,   ActressWatchdog>,(x)=> true),
            (Check<Character,       PirateWatchdog>, (x)=> x.parentName == "Carol_Pirate"),
            (Check<Character,       ActressWatchdog>,(x)=> true),
            (Check<MenuSwitchOutfit,MenuWatchdog>,   (x)=> true),
            (Check<Transform,       PirateWatchdog>, (x)=> NPCInstanceCreator.actressSearchRoots.Contains(x.rootName) && x.parentName == "Carol_Pirate"),
            (Check<Transform,       ActressWatchdog>,(x)=> NPCInstanceCreator.actressSearchRoots.Contains(x.rootName) && x.parentName != "Carol_Pirate")
        };
    }

    protected bool DetectType()
    {
        compData.RefreshParentComponents();
        SetupCheckList();
        foreach (var (func, pred) in checks) if (func.Invoke(pred) is true) return true;
        return false;
    }

    bool Check<SearchType, ResultType>(Predicate<PelvisWatchdog> predicate)
        where SearchType : Component
        where ResultType : PelvisWatchdog
    {
        try { if (!predicate.Invoke(this)) return false; }
        catch (NullReferenceException e) 
        { Log.Warning($"{nameof(ResultType)} predicate caused an exception: {e.Message}"); return false; }

        var component = compData.GetParentComponent(typeof(SearchType));
        if (!component) return false;
        if (GetType() == typeof(ResultType)) return true;
        
        Log.Info($"Type detected as {typeof(SearchType)}, instantiating {typeof(ResultType)}.");
        gameObject.AddComponent<ResultType>().BuildFromExisting(this, component);
        Destroy(this);
        return true;
    }

    public virtual void SetBaseOutfit(Outfit outfit) { }

    public void SetAnimator(Outfit outfit)
    {
        if (this.compData?.Animator is null) { Log.Warning($"null animator when trying to set animator on {this}"); return; }
        Log.Debug($"Setting animator from {outfit}");
        var animator = outfit?.compData?.Controller;
        if (!animator) { Log.Warning("failed to get animator from outfit"); return; }
        
        this.compData.Animator.runtimeAnimatorController = animator;
    }
    public virtual void SetBaseVisibility(bool visible)
    {
        Log.Debug("PelvisWatchdog.SetBaseVisibility()");
        foreach (var mesh in CompData?.allSMRs) { mesh.gameObject.SetActive(visible); }
    }

    public override string ToString() => $"{GetType()}@{rootName}->{grandparentName}({Guid})";
}
