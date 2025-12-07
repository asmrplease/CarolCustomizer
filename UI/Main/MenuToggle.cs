using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Utils;
using Onirism.Ui;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
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
    Scene currentScene;
    GameObject mainMenuPages;
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
        if (currentScene.name != Constants.MenuSceneName) return;
        MainMenuSetMenuState(false);
    }
    void Start()
    {
        OnMenuToggle?.Invoke(false);//uiInstance.Hide();
        currentScene = SceneManager.GetActiveScene();
        if (currentScene.name != Constants.MenuSceneName) return;
        StartCoroutine(OnMainMenuLoaded());
    }

    void OnSceneChange(Scene newScene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) return;

        currentScene = newScene;
        if (currentScene.name != Constants.MenuSceneName) return;
        StartCoroutine(OnMainMenuLoaded()); 
    }

    public void ToggleMenu(bool visible) {
        if (currentScene.name == Constants.MenuSceneName) { MainMenuSetMenuState(visible); return; }
        GameplaySetMenuState(visible);
    }

    void Update()
    {
        if (!uiInstance) return; 

        if (currentScene.name == Constants.MenuSceneName) { MainMenuUpdate(); return; }
        GameplayUpdate();
    }

    bool CheckInput()
    {
        return Input.GetKeyDown(Settings.HotKeys.MenuToggleMouse)
             || Input.GetKeyDown(Settings.HotKeys.MenuToggleKeyboard);
    }

    IEnumerator OnMainMenuLoaded()
    {
        yield return new WaitUntil(() => MainMenuManager.manager);
        Log.Debug("MenuToggle.OnMainMenuLoaded()");

        var pagesFolder = MainMenuManager.manager.transform.Find("PAGES");
        if (!pagesFolder) { Log.Error("didn't find main menu page."); yield return null; }
        if (pagesFolder) { mainMenuPages = pagesFolder.gameObject; }
    }

    void MainMenuUpdate()
    {
        if (!mainMenuPages) return;

        if (CheckInput()) { MainMenuSetMenuState(!IsVisible); return; }
        if (IsVisible && Input.GetKeyDown(KeyCode.Escape)) { MainMenuSetMenuState(false); return; }
    }

    void MainMenuSetMenuState(bool newVisibility)
    {
        //if we're told to switch to the state we're already in, don't do anything lol
        if (newVisibility == IsVisible) { return; }

        //if we are currently visible and asked to go back to regular menu
        if (IsVisible && !newVisibility)
        {
            OnMenuToggle?.Invoke(false);// uiInstance.Hide();
            SetGameMainMenuVisibility(true);
            //mainMenuPages.transform.GetChild(0).gameObject.SetActive(true);
            IsVisible = newVisibility;
            return;
        }

        //if we are not currently visible and asked to become visible
        //var activePage = mainMenuPages.transform.Cast<Transform>().FirstOrDefault(x => x.gameObject.activeSelf);
        //if (!activePage) { Log.Warning("tried to hide menu screen but no active page was found."); return; }
        //if (activePage.GetSiblingIndex() != 0) { Log.Debug("tried to open accUI but we weren't on the main page"); return; }

        OnMenuToggle?.Invoke(true);
        //activePage.gameObject.SetActive(false);
        SetGameMainMenuVisibility(false);
        Log.Debug("hid menu");

        //store the visibility state
        IsVisible = newVisibility;
    }

    void SetGameMainMenuVisibility(bool visible)
    {
        MainMenuTopPanel.I.gameObject.SetActive(visible);
        if (!visible) return;

        var animator = MainMenuTopPanel.I.transform.Find("Screens Container")?.GetChild(0)?.GetComponent<Animator>();
        if (!animator) return;

        animator.SetBool("opened", true);
        animator.Update(0.0f);
        animator.Play("Opened", -1, 1f);
        animator.Update(0.0f);
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

    //void LateUpdate()
    //{
    //    if (!IsVisible) return;
    //    Cursor.visible = true;
    //    Cursor.lockState = CursorLockMode.Confined;
    //}

    void GameplaySetMenuState(bool visible)
    {
        Log.Debug($"GameplaySetMenuState({visible})");
        if (!uiInstance) { Log.Warning("Tried to set ui state on missing uiInstance."); return; }

        //PauseDiary.manager.isPaused = visible;
        //Onirism.Ui.PauseMenu.
        //Onirism.Ui.UiManager.I.isInPauseMenu = visible;
        //GameManager.manager.ToggleCursorState(visible ? CursorLockMode.Confined : CursorLockMode.Locked);
        //var cursorState = visible ? CursorLockMode.Confined : CursorLockMode.Locked;
        //Log.Debug($"Set lockstate to {cursorState}");

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
