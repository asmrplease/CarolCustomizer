using BepInEx.Configuration;
using CarolCustomizer.Utils;
using System;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Settings;
public class GameSettings : IDisposable
{
    ConfigFile config;
    public bool RunInBackground
    {
        set
        {
            RunInBackgroundCE.Value = value;
            Application.runInBackground = value;
        }
        get { return RunInBackgroundCE.Value; }
    }

    readonly ConfigEntry<bool> RunInBackgroundCE;
    public readonly ConfigEntry<KeyCode> Reload;

    public GameSettings(ConfigFile config)
    {
        this.config = config;
        RunInBackgroundCE = config.Bind(
            Constants.Preferences,
            "Run In Background",
            true,
            "Allows the game to continue when focus is lost");
        Reload = config.Bind(
            Constants.Preferences,
            "Stage Reload Hotkey",
            KeyCode.KeypadMinus,
            "Changes the stage reload key to prevent accidental level reloads when typing");
    }

    public void ApplySettings()
    {
        Application.runInBackground = RunInBackgroundCE.Value;
        if (GameManager.manager) GameManager.manager.loadKey = Reload.Value;
        else Log.Warning("GameManager was null when replacing reload hotkey");
    }

    public void ApplyDefaults()
    {
        Application.runInBackground = false;
        if (GameManager.manager) GameManager.manager.loadKey = Constants.DefaultReload;
    }

    public void Dispose() => ApplyDefaults();
}
