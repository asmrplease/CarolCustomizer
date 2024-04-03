using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using CarolCustomizer.UI;
using CarolCustomizer.Assets;
using CarolCustomizer.Models;
using CarolCustomizer.Utils;
using UnityEngine.SceneManagement;
using BepInEx.Configuration;
using UnityEngine.EventSystems;
using CarolCustomizer.Behaviors;

namespace CarolCustomizer;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("Onirism.exe")]
public class CCPlugin : BaseUnityPlugin
{
    #region Static Global Components
    public static List<CarolInstance> playerManagers = new();
    public static List<UIInstance> uiInstances = new();
    public static CarolInstance cutscenePlayer;
    private static TabbedUIAssetLoader uiAssetLoader;
    public static Action<CCPlugin> OnSetupComplete;
    public static GameManagerRewrite gmRewrite;
    #endregion

    #region Config
    int numPlayers = 1;
    ConfigEntry<string> favorites;
    ConfigEntry<KeyCode> mouseMenuToggle;
    ConfigEntry<KeyCode> keyboardMenuToggle;
    ConfigEntry<PointerEventData.InputButton> mouseContextMenu;
    HotKeyConfig hotkeys;
    #endregion

    #region Instance Components
    private Harmony HarmonyInstance;
    private OutfitAssetManager dynamicAssetManager;
    private FavoritesManager favoritesManager;
    private NPCInstanceCreator npcInstances;
    public RecipesManager recipesManager { get; private set; }
    #endregion

    #region Setup
    private void Awake()
    {
        //Setup Static Log
        var logger = new Log(Logger);
        Log.Message("Logger Ready!");

        //Set run in background
        Application.runInBackground = true;
        Log.Debug("run in background set");

        gmRewrite = gameObject.AddComponent<GameManagerRewrite>();

        //Configuration Binding
        favorites = Config.Bind("Preferences", "Favorites", "", "List of favorited accessories.");
        mouseMenuToggle = Config.Bind("Preferences", "Menu Toggle (Mouse)", KeyCode.Mouse3, "Mouse shortcut for opening the accessory menu.");
        keyboardMenuToggle = Config.Bind("Preferences", "Menu Toggle (Keyboard)", KeyCode.Keypad0, "Keyboard shortcut for opening the accessory menu.");
        mouseContextMenu = Config.Bind("Preferences", "Context Menu Button", PointerEventData.InputButton.Right, "Which mouse button activates the context menu.");

        hotkeys = new(mouseMenuToggle.Value, keyboardMenuToggle.Value, mouseContextMenu.Value);
        favoritesManager = new(favorites, Config.Save);

        //Set up clean bone folder
        var cleanBoneFolder = new GameObject().transform;
        cleanBoneFolder.name = "Cleaned Bones";
        cleanBoneFolder.parent = transform;
        BespokeBone.SetCleanFolder(cleanBoneFolder);


        //Instantiate Static Assets
        uiAssetLoader = new();

        //Instantiate Dynamic Assets  
        dynamicAssetManager = new(transform);
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
            uiInstance.Constructor(uiAssetLoader, player, dynamicAssetManager, favoritesManager, recipesManager, hotkeys);

            var menuToggle = gameObject.AddComponent<MenuToggle>();
            menuToggle.Constructor(uiInstance, hotkeys);

            uiInstances.Add(uiInstance);
        }
        cutscenePlayer = playerManagers[0];

        //Instantiate Harmony Instance
        HarmonyInstance = new Harmony("AccessoryModPatch");
        HarmonyInstance.PatchAll();
        Log.Debug("harmony patched");
    }

    private IEnumerator Start()
    {
        Log.Debug("waiting for gamemanager and localization index");
        yield return new WaitUntil(() => GameManager.manager && LocalizationIndex.index is not null);
        Log.Info("Start()");

        //Call this just in case it wasn't already called
        SkeletonManager.SetStandardBones();

        //reduce likelyhood of level reloading when typing
        GameManager.manager.loadKey = KeyCode.KeypadMinus;

        //Load outfits
        Log.Debug("starting hads coroutine");
        StartCoroutine(dynamicAssetManager.LoadAllHaDSOutfits());

        //Trigger player outfit reloads to instsantiate initial watchdog
        OutfitAssetManager.OnHaDSOutfitsLoaded += LoadInitialWatchdogs;

        Log.Info("Invoking Setup() callbacks");
        OnSetupComplete?.Invoke(this);
        Log.Info("Start() complete.");
        yield return null;
    }
    #endregion

    private void LoadInitialWatchdogs()
    {
        if (MainMenuManager.manager)
        {
            var menuCarol = SceneManager.GetActiveScene().GetRootGameObjects().First(x => x.name == "MenuCarolLoader");
            if (!menuCarol) { Log.Warning("Tried to set outfit on main menu but couldn't find menu carol."); return; }

            var mso = menuCarol.GetComponent<MenuSwitchOutfit>();
            if (!mso) { Log.Warning("Failed to get menuswitchoutfit component when trying to init main menu watchdog"); return; }

            string outfitName = mso.transform.GetChild(0).name.DeClone();
            var model = OutfitAssetManager.GetOutfitByAssetName(outfitName);
            if (model is null) { Log.Warning($"coudln't find {outfitName} when performing initial model load"); return; }

            Log.Debug($"setting outfit {outfitName} on main menu carol");
            OutfitManager.SetBaseOutfit(mso, model);
            return;
        }

        if (Entity.players is null) { Log.Debug("no players to refresh."); return; }

        foreach (var entity in Entity.players)
        {
            string outfitName = entity.currentModel.model.name.DeClone();
            var model = OutfitAssetManager.GetOutfitByAssetName(outfitName);
            if (model is null) { Log.Warning($"coudln't find {outfitName} when performing initial model load"); continue; }
            entity.SwapModel(model.storedAsset.gameObject);
        }
    }

    #region Cleanup
    private void OnDestroy()
    {
        Log.Info("Plugin OnDestroy()");

        //restore default load hotkey
        if (GameManager.manager) GameManager.manager.loadKey = KeyCode.Alpha8;

        //save configuration
        Config.Save();

        //Unpatch Harmony
        HarmonyInstance?.UnpatchSelf();

        //Dispose
        this.DisposeFields();

        //Disable runinbackground
        Application.runInBackground = false;
    }
    #endregion
}


