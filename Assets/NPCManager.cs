using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Contracts;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Entity;

namespace CarolCustomizer.Behaviors.Carol;

public enum NPC
{
    Shezara,
    Cherryl,
    Prunelle,
    Blueberry,
    Error,
}

public class NPCManager
{
    static RecipeFileWatcher recipesManager;

    static Dictionary<Guid, CarolInstance> liveBots = [];
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
        if (name.Contains("Blueberry"))   return NPC.Blueberry;
        return NPC.Error;
    }

    public static IEnumerable<NPC> ValidNPCs() => 
        Enum.GetValues(typeof(NPC))
            .Cast<NPC>()
            .Except([NPC.Error]);

    public static void OnBotSpawn(ICarolBot bot)
    {
        Log.Debug("OnBotSpawn()");
        if (!bot.Watchdog()) { Log.Error("Tried to spawn bot will null watchdog!"); return; }
        if (liveBots.ContainsKey(bot.ID())) { Log.Error("Tried to add bot to pool more than once"); return; }

        Log.Debug("OnBotSpawn() Creating new CarolInstance");
        var botInstance = new CarolInstance(NPCManager.folder);
        liveBots.Add(bot.ID(), botInstance);
        liveBots[bot.ID()].NotifySpawned(bot.Watchdog());
        bot.CustomizeBot(GetRandomOutfit(), botInstance.outfitManager);
    }

    public static void OnBotDespawn(ICarolBot bot)
    {
        if (!liveBots.ContainsKey(bot.ID())) { Log.Warning("tried to despawn a bot not in the list"); return; }

        var carolInstance = liveBots[bot.ID()];
        liveBots.Remove(bot.ID());
        carolInstance.Dispose();
    }

    public static void OnNPCAwake(NPCModBehavior npc)
    {
        if (npc.npcType == NPC.Error) { Log.Error("OnNPCAwake() called on an NPC watchdog of type Error"); return; }
        
        var npcInstance = NPCs[npc.npcType];
        if (npcInstance is null) { Log.Error($"Failed to find NPC CarolInstance in NPC dict of type {npc.npcType}"); return; }

        var (enable, recipe) = Settings.Settings.Plugin.customNPCs[npc.npcType];
        if (!enable.Value) { Log.Info($"{npc.npcType} customization disabled."); return; }

        string recipeName = recipe.Value;
        var npcRecipe = recipesManager.Recipes.First(x => x.Name == recipeName);
        if (npcRecipe is null) { Log.Warning($"didn't find {npc.npcType} recipe {recipeName}"); return; }

        npcInstance.NotifySpawned(npc.watchdog);
        Log.Info($"applying {npcRecipe.Name} to NPC");
        RecipeApplier.ActivateRecipe(npcInstance.outfitManager, npcRecipe.Descriptor);
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
