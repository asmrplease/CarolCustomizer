using BepInEx.Logging;
using CarolCustomizer.Assets;
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
    List<(Func<Predicate<PelvisWatchdog>, Result> func, Predicate<PelvisWatchdog> pred)> checks;
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

        Log.Debug($"{this.rootName}.{this.parentName}.PelvisWatchdog.Constructor()");
        CCPlugin.OnPluginDestroy += this.Dispose;
        boneData = this.gameObject.AddComponent<BoneData>().Constructor();
        compData = this.gameObject.AddComponent<CompData>().Constructor();
        magiData = this.gameObject.AddComponent<MagiData>().Constructor();
        animData = this.gameObject.AddComponent<AnimData>().Constructor();
        Behavior = this.gameObject.AddComponent<UnknownCarolBehavior>();
        DetectChanges();
        return this;
    }

    public void Dispose()
    {
        Log.Debug($"{parentName} PelvisWatchdog.Dispose()");
        CCPlugin.OnPluginDestroy -= this.Dispose;
        Behavior.Dispose();
        List<MonoBehaviour> stuff = [boneData, compData, magiData, animData];
        stuff.ForEach(Destroy);
        DestroyImmediate(this);
    }

    void Awake() => Constructor();
    void OnEnable() => DetectChanges();

    void OnDisable() => DetectChanges();
    void OnTransformParentChanged() 
    {
        //This check prevents issues with situations where the player is parented to a new object
        if (Behavior.GetType() == typeof(PlayerModBehavior)) return;

        DetectChanges();
    }
    void DetectChanges()
    {
        Log.Debug("DetectChanges()");
        SetupCheckList();
        checks.Select(tup => tup.func.Invoke(tup.pred))
            .Where(x => x is Result.Detected)
            .First();
        VisibilityChanged?.Invoke(this.Visible);
    }

    enum Result { Detected, NotDetected, Error }

    Result Check<SearchType, ResultType>(Predicate<PelvisWatchdog> predicate)
        where SearchType : Component
        where ResultType : MonoBehaviour, ICustomizable
    {
        try { if (!predicate.Invoke(this)) return Result.NotDetected; }
        catch (NullReferenceException e)
        { Log.Warning($"{nameof(ResultType)} predicate caused an exception: {e.Message}"); return Result.Error; }

        var component = GetComponentInParent<SearchType>(true);
        if (!component) return Result.NotDetected;
        if (Behavior.GetType() == typeof(ResultType)) return Result.Detected;

        Log.Debug($"Type detected as {typeof(SearchType)}, instantiating {typeof(ResultType)}.");
        GetComponents<ICustomizable>().ForEach(x => x.Dispose());
        Behavior = gameObject.AddComponent<ResultType>().Constructor(this);
        return Result.Detected;
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
        (Check<Transform,       OutfitModelBehavior>, (x)=> x.gameObject.scene.buildIndex == -1),
        (Check<Transform,       CarolActressBehavior>,(x)=> NPCManager.GetNPCType(x.parentName) == NPC.Error),
        (Check<Transform,       UnknownCarolBehavior>,(x)=> true)
    ];

}