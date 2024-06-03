using CarolCustomizer.Assets;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using Slate;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs;
public class PelvisWatchdog : MonoBehaviour
{
    readonly Guid Guid = Guid.NewGuid();
    protected Guid SourceGuid;

    [SerializeField]
    protected BoneData boneData;
    public BoneData BoneData { get { return boneData; } }

    [SerializeField]
    protected CompData compData;
    public CompData CompData { get { return compData; } }

    [SerializeField]
    protected MagiData magiData;
    public MagiData MagiData { get { return magiData; } }

    protected string parentName => transform.parent?.name ?? "none";
    protected string grandparentName => transform.parent?.parent?.name ?? "none";
    protected string rootName => transform.root?.name ?? "none";

    protected bool disableAnimator;

    List<(Func<Predicate<PelvisWatchdog>, bool>,
            Predicate<PelvisWatchdog>)> checks;

    virtual public PelvisWatchdog BuildFromExisting(PelvisWatchdog watchdog, Component typeComponent)
    {
        Log.Debug("Watchdog.CopyFromExisting");
        if (!watchdog.BoneData || !watchdog.CompData) Log.Warning($"{watchdog} was missing data when constructing {this}.");
        SourceGuid = watchdog.Guid;
        boneData = watchdog.BoneData;
        compData = watchdog.CompData;
        magiData = watchdog.MagiData;
        return this;
    }

    virtual public void Awake()
    {
        Log.Debug($"{this}.Awake()");
        if (!boneData) boneData = this.gameObject.AddComponent<BoneData>().Constructor();
        if (!compData) compData = this.gameObject.AddComponent<CompData>().Constructor();
        if (!magiData) magiData = this.gameObject.AddComponent<MagiData>().Constructor();
        DetectType();
    }

    virtual protected void OnTransformParentChanged() => DetectType();

    void SetupCheckList() =>
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

    public void DisableAnimator()
    {
        if (!compData.Animator) return;
        disableAnimator = true;
        compData.Animator.enabled = false;
        compData.Animator.Rebind();

        var restingBones = OutfitAssetManager.GetPyjamas().boneData.StandardBones;
        foreach (var resting in restingBones)
        {
            var livebone = boneData.StandardBones[resting.Key];
            livebone.localPosition = resting.Value.localPosition;
            livebone.localRotation = resting.Value.localRotation;
        }

    }

    void LateUpdate()
    {
        if (!disableAnimator) return;
        if (!compData.Animator) return;
        compData.Animator.enabled = false;
    }

    public void EnableAnimator()
    {
        if (!compData.Animator) return;
        disableAnimator = false;
        compData.Animator.enabled = true;
    }

    public virtual void SetBaseOutfit(Outfit outfit) { }

    public virtual void SetAnimator(Outfit outfit) { }

    public virtual void SetHeightOffset(float height) { }
    
    public virtual void SetBaseVisibility(bool visible)
    {
        Log.Debug("PelvisWatchdog.SetBaseVisibility()");
        if (compData?.allSMRs is null) return;
        foreach (var mesh in CompData.allSMRs) { if (mesh?.gameObject) mesh.gameObject.SetActive(visible); }
        Log.Debug("done");
    }

    public override string ToString() => $"{GetType()}@{rootName}->{grandparentName}({Guid})";
}
