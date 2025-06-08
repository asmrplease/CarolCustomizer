using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Carol;

internal enum NPC
{
    Shezara,
    Cherryl,
    Prunelle,
    Error,
}

internal class NPCManager
{
    static RecipeFileWatcher recipesManager;

    static Dictionary<PelvisWatchdog, CarolInstance> liveBots = new();
    public static Dictionary<NPC, CarolInstance> NPCs { get; private set; }

    static Transform folder;

    public static void Constructor(Transform folder, RecipeFileWatcher recipesManager)
    {
        NPCManager.folder = folder;
        NPCManager.recipesManager = recipesManager;
        NPCs = [];
        Enum.GetValues(typeof(NPC))
            .Cast<NPC>()
            .Where(x => x != NPC.Error)
            .ForEach(x => NPCs[x] = new(folder));
    }

    public static NPC GetNPCType(string name)
    {
        if (name == "Carol_Pirate")       return NPC.Shezara;
        if (name.Contains("Cherryl"))     return NPC.Cherryl;
        if (name.Contains("Prunelle"))    return NPC.Prunelle;
        return NPC.Error;
    }

    public static void OnBotSpawn(BotWatchdog pelvis)
    {
        Log.Debug("OnBotSpawn()");
        var botInstance = new CarolInstance(folder);
        liveBots.Add(pelvis, botInstance);
        liveBots[pelvis].NotifySpawned(pelvis);
        pelvis.CustomizeBot(GetRandomOutfit(), botInstance.outfitManager);
    }

    public static void OnNPCAwake(NPCWatchdog pelvis)
    {
        if (pelvis.npcType == NPC.Error) { Log.Error("OnNPCAwake() called on an NPC watchdog of type Error"); return; }
        
        var npcInstance = NPCs[pelvis.npcType];
        if (npcInstance is null) { Log.Error($"Failed to find NPC CarolInstance in NPC dict of type {pelvis.npcType}"); }

        string recipeName = Settings.Settings.Plugin.shezaraRecipe.Value;
        var shezaraRecipe = recipesManager.Recipes
            .First
            (x => x.Name == recipeName);
        if (shezaraRecipe is null) { Log.Warning($"didn't find shezara recipe {recipeName}"); return; }
        npcInstance.NotifySpawned(pelvis);
        Log.Info($"applying {shezaraRecipe.Name} to shezara");
        RecipeApplier.ActivateRecipe(npcInstance.outfitManager, shezaraRecipe.Descriptor);
        //RecipeApplier.ActivateRecipe(npcInstance.outfitManager, GetRandomOutfit().Descriptor);
    }

    public static void OnBotDespawn(PelvisWatchdog pelvis)
    {
        if (!liveBots.ContainsKey(pelvis)) { Log.Warning("tried to despawn a bot not in the list"); return; }
        var manager = liveBots[pelvis];
        liveBots.Remove(pelvis);
        manager.Dispose();
    }

    public static Recipe GetRandomOutfit()
    {
        var random = new System.Random();
        var recipes = recipesManager
            .Recipes
            .Where(x => 
                x.Error == Recipe.Status.NoError
                && x.Name != Constants.AutoSave);
        if (recipes.Count() == 0) return null;

        int index = random.Next(recipes.Count());
        return recipes.ElementAt(index);
    }
}
