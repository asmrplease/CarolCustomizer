using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Contracts;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Utils;
using Slate;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Assets;
internal class ArmatureIdentifier
{
    static readonly List<(Func<PelvisWatchdog, Predicate<PelvisWatchdog>, Result> func, Predicate<PelvisWatchdog> pred)> checks;
    static ArmatureIdentifier() => checks =
        [
            //Check<MonoBehaviour,  ArmatureType>,        (watchdog) => additional detection condition),
            (Check<VirtualCarol,    MPBotArmature>,       (x)=> true),
            (Check<Entity,          PlayerArmature>,      (x)=> x.rootName == "CAROL(Clone)"),
            (Check<Entity,          NPCArmature>,         (x)=> NPCManager.GetNPCType(x.parentName) != NPC.Error),
            (Check<Entity,          SummerSlimeArmature>, (x)=> x.transform.parent.name == "SummerSlimegirl2019"),
            (Check<Entity,          CampaignBotArmature>, (x)=> true),
            (Check<CutsceneActor,   ActressArmature>,     (x)=> true),
            (Check<Character,       NPCArmature>,         (x)=> NPCManager.GetNPCType(x.parentName) != NPC.Error),
            (Check<Character,       ActressArmature>,     (x)=> true),
            (Check<MenuSwitchOutfit,MenuArmature>,        (x)=> true),
            (Check<Transform,       NPCArmature>,         (x)=> NPCManager.GetNPCType(x.parentName) != NPC.Error),
            (Check<Transform,       OutfitArmature>,      (x)=> x.gameObject.scene.buildIndex == -1),
            (Check<Transform,       ActressArmature>,     (x)=> NPCManager.GetNPCType(x.parentName) == NPC.Error),
            (Check<Transform,       UnknownArmature>,     (x)=> true),
        ];

    public static void DetectChanges(PelvisWatchdog watchdog)
    {
        Log.Debug("ArmatureIdentifier.DetectChanges()");
        if (!watchdog) { Log.Warning("DetectChanges() called on a destroyed Watchdog"); return; }

        checks.Select(tup => tup.func.Invoke(watchdog, tup.pred))
            .Where(x => x is Result.Detected)
            .First();
    }

    enum Result { Detected, NotDetected, Error }

    static Result Check<SearchType, ResultType>(PelvisWatchdog armature, Predicate<PelvisWatchdog> predicate)
        where SearchType : Component
        where ResultType : MonoBehaviour, ICarolType
    {
        try { if (!predicate.Invoke(armature)) return Result.NotDetected; }
        catch (NullReferenceException e)
        { Log.Warning($"{nameof(ResultType)} predicate caused an exception: {e.Message}"); return Result.Error; }

        var component = armature.GetComponentInParent<SearchType>(true);
        if (!component) return Result.NotDetected;
        if (armature.Behavior.GetType() == typeof(ResultType)) return Result.Detected;

        Log.Debug($"Type detected as {typeof(SearchType)}, instantiating {typeof(ResultType)}.");
        armature.GetComponents<ICarolType>()
            .ForEach(x => x.Dispose());
        armature.Behavior = armature.gameObject
            .AddComponent<ResultType>()
            .Constructor(armature);
        return Result.Detected;
    }
}
