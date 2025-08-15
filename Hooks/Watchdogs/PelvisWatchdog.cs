using BepInEx.Logging;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using Slate;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs;
public class PelvisWatchdog : MonoBehaviour, IDisposable
{
    public event Action<bool> VisibilityChanged;
    public bool Visible => gameObject.activeInHierarchy;

    [SerializeField]
    AnimData animData;
    public AnimData AnimData { get {  return animData; } }

    [SerializeField]
    BoneData boneData;
    public BoneData BoneData { get { return boneData; } }

    [SerializeField]
    CompData compData;
    public CompData CompData { get { return compData; } }

    [SerializeField]
    MagiData magiData;
    public MagiData MagiData { get { return magiData; } }
    List<(Func<Predicate<PelvisWatchdog>, bool> func, Predicate<PelvisWatchdog> pred)> checks;
    public ICustomizable Behavior { get; private set; }
    string parentName => transform.parent?.name ?? "none";
    string rootName => transform.root?.name ?? "none";
    bool Constructed = false;

    public static PelvisWatchdog GetAddWatchdog(GameObject pelvis)
    {
        if (pelvis.GetComponent<PelvisWatchdog>() is PelvisWatchdog watchdog) return watchdog;
        return pelvis.AddComponent<PelvisWatchdog>().Constructor();
    }

    PelvisWatchdog Constructor()
    {
        if (Constructed) return this;
        Constructed = true;

        Log.Debug($"{this}.Awake()");
        boneData = this.gameObject.GetAddComponent<BoneData>().Constructor();
        compData = this.gameObject.GetAddComponent<CompData>().Constructor();
        magiData = this.gameObject.GetAddComponent<MagiData>().Constructor();
        animData = this.gameObject.GetAddComponent<AnimData>().Constructor();
        Behavior = gameObject.AddComponent<UnknownCarolBehavior>();
        DetectChanges();
        return this;
    }

    void Awake() => Constructor();
    void OnEnable() => DetectChanges();

    void OnDisable() => DetectChanges();
    void OnTransformParentChanged() => DetectChanges();

    public void Dispose()
    {
        Log.Debug($"{parentName} PelvisWatchdog.Destroy()");
        Behavior.Dispose();
        List<MonoBehaviour> stuff = [boneData, compData, magiData, animData];
        stuff.ForEach(Destroy);
        Destroy(this);
    }
    void SetupCheckList() => checks =
        [   
            (Check<VirtualCarol,    MPBotBehavior>,       (x)=> true),
            (Check<Entity,          PlayerModBehavior>,   (x)=> x.rootName == "CAROL(Clone)"),
            (Check<Entity,          NPCModBehavior>,      (x)=> NPCManager.GetNPCType(x.parentName) != NPC.Error),
            (Check<Entity,          CampaignBot>,         (x)=> true),
            (Check<CutsceneActor,   CarolActressBehavior>,(x)=> true),
            (Check<Character,       NPCModBehavior>,      (x)=> NPCManager.GetNPCType(x.parentName) != NPC.Error),
            (Check<Character,       CarolActressBehavior>,(x)=> true),
            (Check<MenuSwitchOutfit,MenuModBehavior>,     (x)=> true),
            (Check<Transform,       NPCModBehavior>,      (x)=> NPCManager.GetNPCType(x.parentName) != NPC.Error),
            (Check<Transform,       CarolActressBehavior>,(x)=> NPCManager.GetNPCType(x.parentName) == NPC.Error),
            (Check<Transform,       OutfitModelBehavior>, (x)=> x.gameObject.scene.buildIndex == -1),
            (Check<Transform,       UnknownCarolBehavior>,(x)=> true)
        ];
    void DetectChanges()
    {
        Log.Debug("DetectChanges()");
        SetupCheckList();
        checks.Select(tup => tup.func.Invoke(tup.pred))
            .Where(x => x is true)
            .Any();
        VisibilityChanged?.Invoke(this.Visible);
    }

    bool Check<SearchType, ResultType>(Predicate<PelvisWatchdog> predicate)
        where SearchType : Component
        where ResultType : MonoBehaviour, ICustomizable
    {
        try { if (!predicate.Invoke(this)) return false; }
        catch (NullReferenceException e)
        { Log.Warning($"{nameof(ResultType)} predicate caused an exception: {e.Message}"); return false; }

        var component = GetComponentInParent<SearchType>(true);
        if (!component) return false;
        if (Behavior.GetType() == typeof(ResultType)) return true;

        Log.Info($"Type detected as {typeof(SearchType)}, instantiating {typeof(ResultType)}.");
        GetComponents<ICustomizable>().ForEach(x => x.Dispose());
        Behavior = gameObject.AddComponent<ResultType>().Constructor(this);
        return true;
    }

}