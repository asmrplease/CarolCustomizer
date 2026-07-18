using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Utils;
using Onirism.Ui;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CarolCustomizer.UI.Main;
public class MenuToggle : MonoBehaviour
{
    #region Dependencies
    UIInstance uiInstance;
    #endregion

    #region State Variables
    public static bool IsVisible { get; private set; } = false;
    string currentScene;
    GameObject screensContainer;
    Animator idk;
    #endregion

    public static event Action<bool> OnMenuToggle;

    public void Constructor(UIInstance uiInstance)
    {
        this.uiInstance = uiInstance;

        SceneManager.sceneLoaded += OnSceneChange;
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneChange;
        if (currentScene != Constants.MenuSceneName) return;
        MainMenuSetMenuState(false);
    }
    void Start()
    {
        OnMenuToggle?.Invoke(false);//uiInstance.Hide();
        currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == Constants.NetworkSceneName) { currentScene = Constants.MenuSceneName; }
        if (currentScene != Constants.MenuSceneName) return;
        StartCoroutine(OnMainMenuLoaded());
    }

    void OnSceneChange(Scene newScene, LoadSceneMode mode)
    {
        if (newScene.name == Constants.NetworkSceneName) return;
        if (mode == LoadSceneMode.Additive) return;

        currentScene = newScene.name;
        if (currentScene != Constants.MenuSceneName) return;

        StartCoroutine(OnMainMenuLoaded()); 
    }

    public void ToggleMenu(bool visible) {
        if (currentScene == Constants.MenuSceneName) { MainMenuSetMenuState(visible); return; }
        GameplaySetMenuState(visible);
    }

    void Update()
    {
        if (!uiInstance) return; 

        if (currentScene == Constants.MenuSceneName) { MainMenuUpdate(); return; }
        GameplayUpdate();
    }

    bool CheckInput()
    {
        return Input.GetKeyDown(Settings.HotKeys.MenuToggleMouse)
            || Input.GetKeyDown(Settings.HotKeys.MenuToggleKeyboard);
    }

    IEnumerator OnMainMenuLoaded()
    {
        yield return new WaitUntil(() => MainMenuTopPanel.I);
        Log.Debug("MenuToggle.OnMainMenuLoaded()");

        var container = MainMenuTopPanel.I.transform.Find("Screens Container");
        if (!container) { Log.Warning("failed to find Screens Container."); yield return null; }

        screensContainer = container.gameObject;
    }

    void MainMenuUpdate()
    {
        if (!screensContainer) return;

        if (CheckInput()) { MainMenuSetMenuState(!IsVisible); return; }
        if (IsVisible && Input.GetKeyDown(KeyCode.Escape)) { MainMenuSetMenuState(false); return; }
    }

    void MainMenuSetMenuState(bool newVisibility)
    {
        //if we're told to switch to the state we're already in, don't do anything lol
        if (newVisibility == IsVisible) { return; }

        //if we are currently visible and asked to go back to regular menu
        SetGameMainMenuVisibility(!newVisibility);
        IsVisible = newVisibility;
        OnMenuToggle?.Invoke(newVisibility);
    }

    void SetGameMainMenuVisibility(bool visible)
    {
        Log.Debug($"SetGameMainMenuVisibility({visible}");
        if (!screensContainer) { Log.Warning("Main menu pages null when trying to set visibility"); return; }
        screensContainer.SetActive(visible);
        if (!visible) return;
        if (screensContainer.transform.GetChild(0)?.GetComponent<Animator>() is not Animator animator) { Log.Warning("main menu animator was null when trying to set visibility"); return; }

        Log.Debug($"{animator.gameObject.name}");
        idk = animator;
        idk.SetBool("opened", true);
    }

    void GameplayUpdate()
    {
        //if we're in a cutscene, exit menu regardless of any other conditions
        if (IsVisible && GameManager.manager.isInCutscene) { GameplaySetMenuState(false); return; }

        //don't change the menu state if we're already in a state change
        if (!PlayerInstances.DefaultPlayer.CanOpenMenu()) return;

        //hide the menu if it's visible and we pause
        if (IsVisible && (Input.GetKeyDown(KeyCode.Escape) || CheckInput()))
        { GameplaySetMenuState(false); return; }

        //if we're in any invalid states for opening the menu
        if (!CheckInput()) return;
        if (GameManager.manager.isInCutscene) return;
        if (Onirism.Ui.UiManager.I.panelStacker.depth > 0) return;

        GameplaySetMenuState(true);
    }


    void GameplaySetMenuState(bool visible)
    {
        Log.Debug($"GameplaySetMenuState({visible})");
        if (!uiInstance) { Log.Warning("Tried to set ui state on missing uiInstance."); return; }

        if (visible) Onirism.Ui.UiManager.I.panelStacker.Stack(uiInstance.lughPanel, true);
        if (!visible) Onirism.Ui.UiManager.I.panelStacker.DestackTopmost(true);

        foreach (GameObject camera in CameraController.cameras)
            camera.GetComponent<CameraController>().enabled = !visible;

        Log.Debug($"Toggled LughPanel to {visible}");
        OnMenuToggle?.Invoke(visible);
        if (visible) PlayerInstances.DefaultPlayer.LockPlayer();
        else PlayerInstances.DefaultPlayer.UnlockPlayer();
        IsVisible = visible;
        Log.Debug("GameplaySetMenuState Completed successfully");
    }
}
