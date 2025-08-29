using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Contracts;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Utils;
using Slate;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarolCustomizer.Assets;
internal class ArmatureIdentifier
{
    //static List<(Func<Predicate<PelvisWatchdog>, Result> func, Predicate<PelvisWatchdog> pred)> checks;
    //static ArmatureIdentifier()
    //{
    //    ArmatureIdentifier.checks =
    //    [
    //        //Check<MonoBehaviour,  ArmaturePurpose>,     (watchdog) => additional detection condition),
    //        //Func<
    //        (Check<VirtualCarol,    MPBotBehavior>,       (x)=> true),
    //        (Check<Entity,          PlayerModBehavior>,   (x)=> x.rootName == "CAROL(Clone)"),
    //        (Check<Entity,          NPCModBehavior>,      (x)=> NPCManager.GetNPCType(x.parentName) != NPC.Error),
    //        (Check<Entity,          SummerSlime>,         (x)=> x.transform.parent.name == "SummerSlimegirl2019"),
    //        (Check<Entity,          CampaignBot>,         (x)=> true),
    //        (Check<CutsceneActor,   CarolActressBehavior>,(x)=> true),
    //        (Check<Character,       NPCModBehavior>,      (x)=> NPCManager.GetNPCType(x.parentName) != NPC.Error),
    //        (Check<Character,       CarolActressBehavior>,(x)=> true),
    //        (Check<MenuSwitchOutfit,MenuModBehavior>,     (x)=> true),
    //        (Check<Transform,       NPCModBehavior>,      (x)=> NPCManager.GetNPCType(x.parentName) != NPC.Error),
    //        (Check<Transform,       OutfitModelBehavior>, (x)=> x.gameObject.scene.buildIndex == -1),
    //        (Check<Transform,       CarolActressBehavior>,(x)=> NPCManager.GetNPCType(x.parentName) == NPC.Error),
    //        (Check<Transform,       UnknownCarolBehavior>,(x)=> true)
    //    ];

    //}

    //enum Result { Detected, NotDetected, Error }

    //static Result Check<SearchType, ResultType>(PelvisWatchdog armature, Predicate<PelvisWatchdog> predicate)
    //    where SearchType : Component
    //    where ResultType : MonoBehaviour, ICustomizable
    //{
    //    try { if (!predicate.Invoke(armature)) return Result.NotDetected; }
    //    catch (NullReferenceException e)
    //    { Log.Warning($"{nameof(ResultType)} predicate caused an exception: {e.Message}"); return Result.Error; }

    //    var component = armature.GetComponentInParent<SearchType>(true);
    //    if (!component) return Result.NotDetected;
    //    if (armature.Behavior.GetType() == typeof(ResultType)) return Result.Detected;

    //    Log.Debug($"Type detected as {typeof(SearchType)}, instantiating {typeof(ResultType)}.");
    //    armature.GetComponents<ICustomizable>().ForEach(x => x.Dispose());
    //    armature.Behavior = armature.gameObject.AddComponent<ResultType>().Constructor(armature);
    //    return Result.Detected;
    //}
}
