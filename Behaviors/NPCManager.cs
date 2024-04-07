using CarolCustomizer.Utils;
using BepInEx;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using CarolCustomizer.Models;
using CarolCustomizer.Hooks.Watchdogs;

namespace CarolCustomizer.Behaviors;
internal class NPCManager 
{
    private static RecipesManager recipesManager;

    private static Dictionary<PelvisWatchdog, CarolInstance> liveBots = new();
    public static CarolInstance shezaraInstance { get; private set; }

    private static GameObject folder;

    public static void Constructor(GameObject folder, RecipesManager recipesManager)
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

        Log.Debug("applying random recipe");
        var randomRecipe = NPCManager.GetRandomOutfit();
        RecipeApplier.ActivateRecipe(botInstance.outfitManager, randomRecipe.Descriptor);
        pelvis.SetBotName(randomRecipe.Name);
    }

    public static void OnShezaraAwake(PelvisWatchdog pelvis)
    {
        var shezaraRecipe = recipesManager.Recipes.First(x => x.Name == Constants.Shezara);
        if (shezaraRecipe is null) { Log.Warning("didn't find shezara recipe"); return; }
        shezaraInstance.NotifySpawned(pelvis);
        RecipeApplier.ActivateRecipe(shezaraInstance.outfitManager, shezaraRecipe.Descriptor);
    }

    public static void OnShezaraDestroyed(PelvisWatchdog pelvis)
    {
        Log.Debug($"ShezaraDestroyed {pelvis}");
        //if (shezaraInstance.RestorePrevious(pelvis)) return;
        //shezaraInstance.outfitManager.Dispose();
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
        var recipes = recipesManager.Recipes.Where(x=>x.Error == Recipe.Status.NoError);
        if (recipes.Count() == 0) return null;
        int index = random.Next(recipes.Count());
        return recipes.ElementAt(index);
    }
}
