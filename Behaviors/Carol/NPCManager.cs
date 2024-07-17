using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Carol;
internal class NPCManager
{
    static RecipeFileWatcher recipesManager;

    static Dictionary<PelvisWatchdog, CarolInstance> liveBots = new();
    public static CarolInstance shezaraInstance { get; private set; }

    static GameObject folder;

    public static void Constructor(GameObject folder, RecipeFileWatcher recipesManager)
    {
        NPCManager.folder = folder;
        NPCManager.recipesManager = recipesManager;
        shezaraInstance = new(folder);
    }

    public static void OnBotSpawn(BotWatchdog pelvis)
    {
        Log.Debug("OnBotSpawn()");
        var botInstance = new CarolInstance(folder);
        liveBots.Add(pelvis, botInstance);
        liveBots[pelvis].NotifySpawned(pelvis);
        pelvis.CustomizeBot(GetRandomOutfit(), botInstance.outfitManager);
    }

    public static void OnShezaraAwake(PelvisWatchdog pelvis)
    {
        string recipeName = Settings.Settings.Plugin.shezaraRecipe.Value;
        var shezaraRecipe = recipesManager.Recipes
            .First
            (x => x.Name == recipeName);
        if (shezaraRecipe is null) { Log.Warning($"didn't find shezara recipe {recipeName}"); return; }
        shezaraInstance.NotifySpawned(pelvis);
        Log.Info($"applying {shezaraRecipe.Name} to shezara");
        RecipeApplier.ActivateRecipe(shezaraInstance.outfitManager, shezaraRecipe.Descriptor);
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
