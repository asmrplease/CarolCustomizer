using BepInEx.Configuration;
using CarolCustomizer.Utils;
using System;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Settings;
public class GameSettings : IDisposable
{
    ConfigFile config;
    ConfigEntry<bool> RunInBackground;
    ConfigEntry<KeyCode> Reload;

    public GameSettings(ConfigFile config)
    {
        this.config = config;
        RunInBackground = config.Bind(
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
        Application.runInBackground = RunInBackground.Value;
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
