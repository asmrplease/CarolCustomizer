using BepInEx;
using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Hooks;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.UI.Main;
using CarolCustomizer.Utils;
using FaceCam.Behaviors;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("Onirism.exe")]
public class CCPlugin : BaseUnityPlugin
{
    #region Static Global Components
    public static MonoBehaviour CoroutineRunner;
    public static List<PlayerCarolInstance> playerManagers = new();
    public static List<UIInstance> uiInstances = new();
    public static CarolInstance cutscenePlayer;
    public static event Action<CCPlugin> OnSetupComplete;
    public static RecipeFileWatcher recipesManager { get; private set; }
    public static ThumbnailCamera thumbnailCamera;
    #endregion

    //the dream is that we can increase this, but that's not really working out rn
    int numPlayers = 1;

    #region Instance Components
    Harmony HarmonyInstance;
    OutfitAssetManager outfitAssetManager;
    NPCInstanceCreator npcInstances;
    HaDSOutfitLoader outfitLoader;
    SaveDataAdjuster saveAdjuster;
    UIAssetLoader uiAssetLoader;
    

    #endregion

    #region Setup
    void Awake()
    {
        new Log(Logger);
        Log.Message("Logger Ready!");
        //this.gameObject.AddComponent<DebugOnKeypress>().Constructor(KeyCode.Tab, "----------------------------------------------------------------------------------------------", Log.Info);
        CoroutineRunner = this;

        Settings.Constructor(Config);

        saveAdjuster = new();
        uiAssetLoader = new();
        outfitAssetManager = new(transform);
        outfitLoader = new();
        recipesManager = new(Constants.RecipeFolderPath);
        NPCManager.Constructor(gameObject, recipesManager);
        npcInstances = new(transform);

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

    IEnumerator Start()
    {
        Log.Debug("waiting for gamemanager and localization index");
        yield return new WaitUntil(() => GameManager.manager && LocalizationIndex.index is not null);
        Log.Info("Start()");

        if (!CommonBones.Ready) CommonBones.SetCommonBones();

        Settings.Game.ApplySettings();

        Log.Debug("starting hads coroutine");
        OutfitAssetManager.OnOutfitSetLoaded += LoadInitialWatchdogs;
        StartCoroutine(outfitLoader.LoadAllHaDSOutfits());
        if (!Camera.main) yield return new WaitUntil(() => Camera.main);

        thumbnailCamera = GameObject
            .Instantiate(Camera.main.gameObject, this.transform)
            .AddComponent<ThumbnailCamera>();
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
            var menu = OnirismExtensions.GetMenuCarolPelvis();
            if (menu.GetComponent<PelvisWatchdog>()) return;

            menu.AddComponent<PelvisWatchdog>();
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
        playerManagers
            .Where(x => x is not null)
            .ForEach(x => x.Dispose());
        uiAssetLoader.Dispose();
        outfitAssetManager.Dispose();
        npcInstances.Dispose();
        outfitLoader.Dispose();
        AccessoryDissolver.Dispose();
        Log.Info("Customizer unloaded.");
    }
}
