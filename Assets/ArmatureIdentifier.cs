using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Contracts;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Hooks.Watchdogs.UnhandledArmatures;
using CarolCustomizer.Utils;
using Onirism.Ui;
using Slate;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Assets;
internal class ArmatureIdentifier
{
    /// <summary>
    /// An ordered list of functions to evaluate to determine armature type.
    /// The function evaluates a PelvisWatchdog, a Component type to search for, an armature behavior to instantiate on success, and an additional predicate, then returns a Result (yes, no, error).
    /// The list items are tuples, bundling the reference to the typed function with their corresponding predicate.
    /// Each line acts like: if (watchdog.GetComponentInParent<SearchType>()) && predicate.Invoke(watchdog)) {watchdog.AddComponent<ResultType>();}
    /// Check<Transform>() and "(x) => true" always evaluate to true.
    /// The list is checked in order, so in the event an armature would pass more than one test, it is assigned based on the first matching rule.
    /// This convoluted setup is meant to make the checking process safe to call, and easy to adjust. 
    /// </summary>
    static readonly List<(Func<PelvisWatchdog, Predicate<PelvisWatchdog>, Result> func, Predicate<PelvisWatchdog> pred)> checks;
    static ArmatureIdentifier() => checks =
    [
        ///Check<SearchType,    ResultType>,          (watchdog) => additional detection condition),             //Purpose
        (Check<Transform,       SpoilerArmature>,     (x) => x.parentName.Contains("Carol_Adult")),              //Adult Carol
        (Check<MenuSwitchOutfit,MenuArmature>,        (x) => true),                                              //Menu
        (Check<VirtualCarol,    MPBotArmature>,       (x) => true),                                              //Multiplayer Bots
        (Check<Transform,       ShopArmature>,        (x) => x.rootName   == "GameManager"),                     //Shop Dummy
        (Check<Entity,          PCBBArmature>,        (x) => x.parentName == "BlueberryPlayerRig"),              //Playable Blueberry
        (Check<Entity,          PlayerArmature>,      (x) => x.rootName.StartsWith("Carol - Player")),           //Standard Player
        (Check<Entity,          NPCArmature>,         (x) => NPCManager.GetNPCType(x.parentName) != NPC.Error),  //NPC Entities
        (Check<Entity,          SummerSlimeArmature>, (x) => x.parentName == "SummerSlimegirl2019"),             //Summer Slimes
        (Check<Entity,          SpaceCorpArmature>,   (x) => x.rootName   == "Corp_space(Clone)"),               //Space station enemies
        (Check<Entity,          WitchArmature>,       (x) => x.parentName == "witchStudent"),                    //Cursed Forest enemies
        (Check<Entity,          CampaignBotArmature>, (x) => x.parentName == "Carol_Robot"),                     //Campaign bots
        (Check<CutsceneActor,   ActressArmature>,     (x) => true),                                              //Carol Actress
        (Check<Character,       NPCArmature>,         (x) => NPCManager.GetNPCType(x.parentName) != NPC.Error),  //NPC Actresses
        (Check<Character,       ActressArmature>,     (x) => true),                                              //Carol Actress
        (Check<Transform,       NPCArmature>,         (x) => NPCManager.GetNPCType(x.parentName) != NPC.Error),  //NPCs 
        (Check<Transform,       OutfitArmature>,      (x) => x.gameObject.scene.buildIndex == -1),               //Outfit references
        (Check<Transform,       ToySoldierArmature>,  (x) => x.parentName.Contains("Carol_Toysoldier")),         //ChristmasArenaDecoration
        (Check<Transform,       RobonickelArmature>,  (x) => x.parentName == "Carol_Bionicle"),                  //ChristmasArenaDecoration
        (Check<Transform,       ResortNPCArmature>,   (x) => x.rootName   == "CHARACTERS"),                      //Resort NPCs
        (Check<Transform,       ActressArmature>,     (x) => NPCManager.GetNPCType(x.parentName) == NPC.Error),  //Carol Actress? Seems to be causing trouble
        (Check<Transform,       MenuDummyArmature>,   (x) => x.rootName   == "MenuDummySvc"),                    //Menu Dummy
        (Check<Transform,       UnknownArmature>,     (x) => true),                                              //Catch-All
    ];

    public static void DetectChanges(PelvisWatchdog watchdog)
    {
        if (!watchdog) { Log.Warning("DetectChanges() called on a destroyed Watchdog"); return; }

        checks
            .Select(check => check.func.Invoke(watchdog, check.pred))
            .First(x => x is Result.Detected);
    }

    enum Result { Detected, NotDetected, Error }

    static Result Check<SearchType, ResultType>(PelvisWatchdog armature, Predicate<PelvisWatchdog> predicate)
        where SearchType : Component
        where ResultType : MonoBehaviour, ICarolType
    {
        try { if (!predicate.Invoke(armature)) return Result.NotDetected; }
        catch (NullReferenceException e){ Log.Error($"{nameof(ResultType)} predicate caused an exception: {e.Message}"); return Result.Error; }

        var component = armature.GetComponentInParent<SearchType>(true);
        if (!component) return Result.NotDetected;
        if (armature.Behavior.GetType() == typeof(ResultType)) return Result.Detected;

        Log.Debug($"Type detected as {typeof(SearchType)}, instantiating {typeof(ResultType)}.");
        armature.GetComponents<ICarolType>().ForEach(x => x.Dispose());
        armature.Behavior = armature.gameObject
            .AddComponent<ResultType>()
            .Constructor(armature);
        return Result.Detected;
    }
}