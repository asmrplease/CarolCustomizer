﻿using BepInEx;
using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Hooks;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.UI.Main;
using CarolCustomizer.Utils;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CarolCustomizer;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("Onirism.exe")]
public class CCPlugin : BaseUnityPlugin
{
    #region Static Global Components
    public static List<CarolInstance> playerManagers = new();
    public static List<UIInstance> uiInstances = new();
    public static CarolInstance cutscenePlayer;
    public static Action<CCPlugin> OnSetupComplete;
    public static GameManagerRewrite gmRewrite;
    public static RecipesManager recipesManager { get; private set; }
    #endregion

    //the dream is that we can increase this, but that's not really working out rn
    int numPlayers = 1;

    #region Instance Components
    Harmony HarmonyInstance;
    OutfitAssetManager outfitAssetManager;
    NPCInstanceCreator npcInstances;
    IntroCutsceneFixBehavior introFix;
    HaDSOutfitLoader outfitLoader;
    SaveDataAdjuster saveAdjuster;
    UIAssetLoader uiAssetLoader;
    #endregion

    #region Setup
    private void Awake()
    {
        new Log(Logger);
        Log.Message("Logger Ready!");

        Settings.Constructor(Config);
        saveAdjuster = new();

        gmRewrite = gameObject.AddComponent<GameManagerRewrite>();

        //Set up clean bone folder
        var cleanBoneFolder = new GameObject().transform;
        cleanBoneFolder.name = "Cleaned Bones";
        cleanBoneFolder.parent = transform;
        cleanBoneFolder.gameObject.SetActive(false); 
        BespokeBone.SetCleanFolder(cleanBoneFolder);

        //Instantiate Static Assets
        uiAssetLoader = new();

        //Instantiate Dynamic Assets  
        outfitAssetManager = new(transform);
        outfitLoader = new();
        introFix = new(this.gameObject);
        recipesManager = new(Constants.RecipeFolderPath);

        //Set up NPC manager
        NPCManager.Constructor(gameObject, recipesManager);
        npcInstances = new(transform);

        //Instantiate Player Managers
        for (int i = 1; i <= numPlayers; i++)
        {
            Log.Debug($"Making playerManager[{i}]...");
            var player = new PlayerCarolInstance(gameObject);
            playerManagers.Add(player);

            var uiInstance = gameObject.AddComponent<UIInstance>();
            uiInstance.Constructor(uiAssetLoader, player, recipesManager);

            var menuToggle = gameObject.AddComponent<MenuToggle>();
            menuToggle.Constructor(uiInstance);

            uiInstances.Add(uiInstance);
        }
        cutscenePlayer = playerManagers[0];

        //Instantiate Harmony Instance
        HarmonyInstance = new Harmony("AccessoryModPatch");
        HarmonyInstance.PatchAll();
        Log.Debug("harmony patched");
        Log.Info("CCPlugin.Awake() success.");
    }

    private IEnumerator Start()
    {
        Log.Debug("waiting for gamemanager and localization index");
        yield return new WaitUntil(() => GameManager.manager && LocalizationIndex.index is not null);
        Log.Info("Start()");

        if (SkeletonManager.CommonBones is null) SkeletonManager.SetCommonBones();

        Settings.Game.ApplySettings();

        Log.Debug("starting hads coroutine");
        OutfitAssetManager.OnOutfitSetLoaded += LoadInitialWatchdogs;
        StartCoroutine(outfitLoader.LoadAllHaDSOutfits());
        StartCoroutine(SaveDataAdjuster.EnsurePyjamas());

        Log.Debug("Invoking CCPlugin.Start() callbacks");
        OnSetupComplete?.Invoke(this);
        Log.Info("Start() complete.");
        yield return null;
    }
    #endregion

    void LoadInitialWatchdogs()
    {
        if (MainMenuManager.manager)
        {
            OnirismExtensions
                .GetMenuCarolPelvis()
                .AddComponent<PelvisWatchdog>();
            return;
        }

        if (Entity.players is null) { Log.Debug("no players to refresh."); return; }

        foreach (var player in Entity.players)
        {
            player
                .transform
                .RecursiveFindTransform(x => x.name == "CarolPelvis")
                .gameObject
                .AddComponent<PelvisWatchdog>();
        }
    }

    void OnDisable()
    {
        Log.Info("Plugin OnDisable()");
        Config.Save();
        HarmonyInstance?.UnpatchSelf();
        Settings.Dispose();
        foreach(var player in playerManagers) { if (player is not null) player.Dispose(); }
        uiAssetLoader.Dispose();
        outfitAssetManager.Dispose();
        npcInstances.Dispose();
        introFix.Dispose();
        outfitLoader.Dispose();
    }
}
