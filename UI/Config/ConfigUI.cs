﻿using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.UI.Main;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Config;
public class ConfigUI : MonoBehaviour
{
    #region Transform Addresses
    const string KeyboardToggleAddress = "Menu Toggle Keyboard Key/Dropdown";
    const string MouseToggleAddress = "Menu Toggle Mouse Key/Dropdown";
    const string ReloadStageAddress = "Reload Stage Replacement Key/Dropdown";

    const string LogFolderButtonAddress = "OpenLogFolder";
    const string LogFileButtonAddress = "OpenLogFile";
    const string ClearFavoritesButtonAddress = "ClearFavorites";

    const string RunInBackgroundToggleAddress = "RunInBackground/Toggle";
    const string RightMouseButtonToggle = "MouseButton/Toggles/Right";
    #endregion

    MessageDialogue dialoge;

    public void Constructor(MessageDialogue dialogue)
    {
        this.dialoge = dialogue;

        transform
            .Find(LogFolderButtonAddress)
            .GetComponent<Button>()
            .onClick
            .AddListener(OpenLogFolder);
        
        transform 
            .Find(LogFileButtonAddress)
            .GetComponent<Button>()
            .onClick
            .AddListener(OpenLogFile);

        transform
            .Find(ClearFavoritesButtonAddress)
            .GetComponent<Button>()
            .onClick
            .AddListener(ConfirmClearFavorites);

        transform
            .Find(KeyboardToggleAddress)
            .gameObject
            .AddComponent<KeyCodeDropDown>()
            .Constructor(Settings.HotKeys.keyboardMenuToggle);

        transform
            .Find(MouseToggleAddress)
            .gameObject
            .AddComponent<KeyCodeDropDown>()
            .Constructor(Settings.HotKeys.mouseMenuToggle);

        transform
            .Find(ReloadStageAddress)
            .gameObject
            .AddComponent<KeyCodeDropDown>()
            .Constructor(Settings.Game.Reload);

        var RMBToggle = transform
            .Find(RightMouseButtonToggle)
            .GetComponent<Toggle>();
        RMBToggle
            .onValueChanged
            .AddListener(OnRightToggle);
        RMBToggle
            .SetIsOnWithoutNotify(
            Settings.HotKeys.mouseContextMenu.Value == PointerEventData.InputButton.Right);

        var runInBackground = transform
            .Find(RunInBackgroundToggleAddress)
            .GetComponent<Toggle>();
        runInBackground
            .onValueChanged
            .AddListener
            ((x) => Settings.Game.RunInBackground = x);
        runInBackground
            .SetIsOnWithoutNotify(
            Settings.Game.RunInBackground);
    }

    private void OnRightToggle(bool state)
    {
        Settings.HotKeys.mouseContextMenu.Value = 
            state?
            PointerEventData.InputButton.Right:
            PointerEventData.InputButton.Middle;
    }

    private void ConfirmClearFavorites()
    {
        Log.Debug("ConfirmClearFavorites()");
        dialoge.Show(
            "Are you sure you want to permanently clear your favorites?",
            "Yes.", Settings.Favorites.ResetFavorites,
            "Cancel!", null);
    }

    private void OpenLogFolder()
    {
        //string argument = @"/select, " + "\"" + Constants.LogFileName + "\""; //TODO get highlighting the file working?
        try { Process.Start(Constants.BepInExFolderPath); }
        catch (Win32Exception e) { Log.Warning(e.Message); }
    }

    private void OpenLogFile()
    {
        try { Process.Start(Constants.LogFilePath); }
        catch (Win32Exception e) { Log.Warning(e.Message); }
    }
}