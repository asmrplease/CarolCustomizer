﻿using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CarolCustomizer.UI;
public class MenuToggle : MonoBehaviour
{
    #region Dependencies
    UIInstance uiInstance;
    #endregion

    #region State Variables
    bool isVisible = false;
    Scene currentScene;
    GameObject mainMenuPages;
    #endregion

    //TODO: fix input still going to UI when hidden... how? is there an interactable flag we can set?

    public void Constructor(UIInstance uiInstance)
    {
        this.uiInstance = uiInstance;
        SceneManager.sceneLoaded += OnSceneChange;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneChange;
    }
    private void Start()
    {
        uiInstance.Hide();
        currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == Constants.MenuSceneName) { StartCoroutine(OnMainMenuLoaded()); }
    }

    private void OnSceneChange(Scene newScene, LoadSceneMode mode)
    {
        currentScene = newScene;
        if (currentScene.name == Constants.MenuSceneName) { StartCoroutine(OnMainMenuLoaded()); }
    }

    private void Update()
    {
        if (!uiInstance) return;
        if (PauseDiary.manager) { GameplayUpdate(); return; }
        if (currentScene.name == Constants.MenuSceneName) { MainMenuUpdate(); return; }
    }

    private bool CheckInput()
    {
        return Input.GetKeyDown(Settings.HotKeys.MenuToggleMouse)
             || Input.GetKeyDown(Settings.HotKeys.MenuToggleKeyboard);
    }

    private IEnumerator OnMainMenuLoaded()
    {
        yield return new WaitUntil(() => MainMenuManager.manager);
        Log.Debug("OnMainMenuLoaded()");

        var pagesFolder = MainMenuManager.manager.transform.Find("PAGES");
        if (!pagesFolder) { Log.Error("didn't find main menu page."); yield return null; }
        if (pagesFolder) { mainMenuPages = pagesFolder.gameObject; }
    }

    private void MainMenuUpdate()
    {
        if (!mainMenuPages) return;

        if (CheckInput()) { MainMenuSetMenuState(!isVisible); return; }
        if (isVisible && Input.GetKeyDown(KeyCode.Escape)) { MainMenuSetMenuState(false); return; }
    }

    private void MainMenuSetMenuState(bool newVisibility)
    {
        //if we're told to switch to the state we're already in, don't do anything lol
        if (newVisibility == isVisible) { return; }

        //if we are currently visible and asked to go back to regular menu
        if (isVisible && !newVisibility)
        {
            uiInstance.Hide();
            mainMenuPages.transform.GetChild(0).gameObject.SetActive(true);
            isVisible = newVisibility;
            return;
        }

        //if we are not currently visible and asked to become visible
        var activePage = mainMenuPages.transform.Cast<Transform>().First(x => x.gameObject.activeSelf);
        if (!activePage) { Log.Warning("tried to hide menu screen but no active page was found."); return; }
        if (activePage.GetSiblingIndex() != 0) { Log.Debug("tried to open accUI but we weren't on the main page"); return; }

        uiInstance.Show();
        activePage.gameObject.SetActive(false);
        Log.Debug("hid menu");

        //store the visibility state
        isVisible = newVisibility;
    }

    private void GameplayUpdate()
    {
        //if we're in a cutscene, exit menu regardless of any other conditions
        if (isVisible && GameManager.manager.isInCutscene) { GameplaySetMenuState(false); return; }

        //don't change the menu state if we're already in a state change
        var player = uiInstance.playerManager;
        if (!player.CanOpenMenu()) return;

        //hide the menu if it's visible and we pause
        if (isVisible && (Input.GetKeyDown(KeyCode.Escape) || CheckInput()))
        { GameplaySetMenuState(false); return; }

        //if we're in any invalid states for opening the menu
        if (!CheckInput()) return;
        if (GameManager.manager.isInCutscene) return;

        GameplaySetMenuState(true);
    }

    public void GameplaySetMenuState(bool visible)
    {
        Log.Debug($"GameplaySetMenuState({visible})");
        if (!uiInstance) { Log.Warning("Tried to set ui state on missing uiInstance."); return; }

        //set game pause diary state
        if (PauseDiary.manager) PauseDiary.manager.isPaused = visible;

        //set ui visibility?
        if (visible) uiInstance.Show(); else uiInstance.Hide();

        //Set player state
        if (visible) uiInstance.playerManager.LockPlayer();
        else uiInstance.playerManager.UnlockPlayer();

        //store the visibility state
        isVisible = visible;
    }
}