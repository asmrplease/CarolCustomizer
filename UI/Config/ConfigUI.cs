using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Config;
public class ConfigUI : MonoBehaviour
{
    const string KeyboardToggleAddress = "Menu Toggle Keyboard Key/Dropdown";
    const string MouseToggleAddress = "Menu Toggle Mouse Key/Dropdown";
    const string ReloadStageAddress = "Reload Stage Replacement Key/Dropdown";
    const string LogFolderButtonAddress = "OpenLogFolder";
    const string LogFileButtonAddress = "OpenLogFile";

    public void Constructor()
    {
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
