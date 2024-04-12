using CarolCustomizer.Models;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Slate;
using CarolCustomizer.Assets;

namespace CarolCustomizer.Hooks.Watchdogs;
public class PelvisWatchdog : MonoBehaviour
{   
    readonly Guid Guid = Guid.NewGuid();
    protected Guid SourceGuid;

    [SerializeField]
    BoneData boneData;
    public BoneData BoneData { get { return boneData; } }

    [SerializeField]
    MeshData meshData;
    public MeshData MeshData { get { return meshData; } }

    string parentName => transform.parent?.name ?? "none";
    string grandparentName => transform.parent?.parent?.name ?? "none";
    string rootName => transform.root?.name ?? "none";

    public CoopModelToggle[] coopToggles { get; private set; }

    Dictionary<Type, Component> parentComponents = new();

    List<(Func<Predicate<PelvisWatchdog>, bool>,
    Predicate<PelvisWatchdog>)> checks;

    virtual public PelvisWatchdog BuildFromExisting(PelvisWatchdog watchdog, Component typeComponent)
    {
        Log.Debug("Watchdog.CopyFromExisting");
        if (!watchdog.BoneData || !watchdog.MeshData) Log.Warning($"{watchdog} was missing data when constructing {this}.");
        SourceGuid = watchdog.Guid;
        boneData = watchdog.BoneData;
        meshData = watchdog.MeshData;
        coopToggles = watchdog.coopToggles;
        return this;
    }

    virtual public void Awake()
    {
        Log.Debug($"{this}.Awake()");
        if (!boneData) boneData = this.gameObject.AddComponent<BoneData>().Constructor();
        if (!meshData) meshData = this.gameObject.AddComponent<MeshData>().Constructor();

        coopToggles = transform.parent.GetComponentsInChildren<CoopModelToggle>(true);
        foreach (var toggle in coopToggles) { toggle.enabled = false; }
        DetectType();
    }

    virtual protected void OnTransformParentChanged() => DetectType(); 

    private void RefreshParentComponents()
    {
        parentComponents = GetComponentsInParent<Component>(true)
            .ToDictionaryOverwrite(x => x.GetType());
    }

    void SetupCheckList()
    {
        checks = new()
        {
            (Check<VirtualCarol,    MPBotWatchdog>,  (x)=> true),
            (Check<Entity,          PlayerWatchdog>, (x)=> x.rootName == "CAROL(Clone)"),
            (Check<Entity,          BotWatchdog>,    (x)=> true), 
            (Check<CutsceneActor,   ActressWatchdog>, (x)=> true),
            (Check<Character,       PirateWatchdog>, (x)=> x.parentName == "Carol_Pirate"),
            (Check<Character,       ActressWatchdog>, (x)=> true),
            (Check<MenuSwitchOutfit,  MenuWatchdog>, (x)=> true),
            //TODO: these checks may have unexpected results when the base outfit is a pirate
            (Check<Transform,       PirateWatchdog>, (x)=> NPCInstanceCreator.actressSearchRoots.Contains(x.rootName) && x.parentName == "Carol_Pirate"),
            (Check<Transform,       ActressWatchdog>, (x)=> NPCInstanceCreator.actressSearchRoots.Contains(x.rootName) && x.parentName != "Carol_Pirate")
        };
    }

    protected bool DetectType()
    {
        RefreshParentComponents();
        SetupCheckList();
        foreach (var (func, pred) in checks) if (func.Invoke(pred)) return true;
        return false;
    }

    bool Check<SearchType, ResultType>(Predicate<PelvisWatchdog> predicate)
        where SearchType : Component
        where ResultType : PelvisWatchdog
    {
        try { if (!predicate.Invoke(this)) return false; }
        catch (NullReferenceException e) { Log.Warning("predicate caused an exception"); return false; }
        
        if (!parentComponents.ContainsKey(typeof(SearchType))) return false;
        if (GetType() == typeof(ResultType)) return true;
        
        Log.Info($"Type detected as {typeof(SearchType)}, instantiating {typeof(ResultType)}.");
        var component = parentComponents[typeof(SearchType)];
        gameObject.AddComponent<ResultType>().BuildFromExisting(this, component);
        Destroy(this);
        return true;
    }

    public virtual void SetBaseOutfit(Outfit outfit) { }
    public virtual void SetBaseVisibilty(bool visible)
    {
        foreach (var mesh in MeshData?.baseMeshes) { mesh.gameObject.SetActive(visible); }
    }

    public override string ToString() => $"{GetType()}@{rootName}->{grandparentName}({Guid})";
}
