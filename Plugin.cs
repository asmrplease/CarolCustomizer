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
using System.Linq;
using UnityEngine;

namespace CarolCustomizer;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("Onirism.exe")]
public class CCPlugin : BaseUnityPlugin
{
    #region Static Global Components
    public static MonoBehaviour CoroutineRunner;
    public static UIInstance uiInstance;
    public static event Action<CCPlugin> OnSetupComplete;
    public static RecipeFileWatcher recipesManager { get; private set; }
    public static ThumbnailCamera thumbnailCamera;
    #endregion

    #region Instance Components
    Harmony HarmonyInstance;
    OutfitAssetManager outfitAssetManager;
    NPCInstanceCreator npcInstances;
    HaDSOutfitLoader outfitLoader;
    SaveDataAdjuster saveAdjuster;
    UIAssetLoader uiAssetLoader;
    PlayerInstances players;
    Transform folder;
    #endregion

    #region Setup
    void Awake()
    {
        new Log(Logger);
        Log.Message("CCPlugin.Awake() Start");
        folder = new GameObject("CCPlugin").transform;
        folder.parent = this.transform;
        CoroutineRunner = this;
        Settings.Constructor(Config);
        saveAdjuster = new();
        uiAssetLoader = new();
        outfitAssetManager = new(folder);
        outfitLoader = new();
        recipesManager = new(Constants.RecipeFolderPath);
        players = new(folder);
        NPCManager.Constructor(folder, recipesManager);
        npcInstances = new(folder);
        uiInstance = folder
            .gameObject
            .AddComponent<UIInstance>()
            .Constructor(uiAssetLoader, recipesManager);
        HarmonyInstance = new Harmony("AccessoryModPatch");
        HarmonyInstance.PatchAll();
        Log.Info("CCPlugin.Awake() Success!");
    }

    IEnumerator Start()
    {
        Log.Debug("waiting for gamemanager and localization index");
        yield return new WaitUntil(() => GameManager.manager && LocalizationIndex.index is not null);
        
        Log.Info("Start()");
        if (!CommonBones.Ready) CommonBones.SetCommonBones();
        Settings.Game.ApplySettings();
        OutfitAssetManager.OnOutfitSetLoaded += LoadInitialWatchdogs;
        StartCoroutine(outfitLoader.LoadAllHaDSOutfits());
        if (!Camera.main) yield return new WaitUntil(() => Camera.main);

        thumbnailCamera = GameObject
            .Instantiate(Camera.main.gameObject, folder.transform)
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
        uiAssetLoader.Dispose();
        Config.Save();
        HarmonyInstance?.UnpatchSelf();
        Settings.Dispose();
        players.Dispose();
        outfitAssetManager.Dispose();
        npcInstances.Dispose();
        outfitLoader.Dispose();
        AccessoryDissolver.Dispose();
        Log.Info("Customizer unloaded.");
    }
}
