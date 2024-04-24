using BepInEx.Configuration;
using CarolCustomizer.Utils;
using System;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Settings;
public class GameSettings : IDisposable
{
    public readonly ConfigEntry<bool> RunInBackgroundCE;
    public readonly ConfigEntry<KeyCode> Reload;

    public GameSettings(ConfigFile config)
    {
        RunInBackgroundCE = config.Bind(
            Constants.Preferences,
            "Run In Background",
            true,
            "Allows the game to continue when focus is lost");
        RunInBackgroundCE.SettingChanged += OnSettingChanged;

        Reload = config.Bind(
            Constants.Preferences,
            "Stage Reload Hotkey",
            KeyCode.KeypadMinus,
            "Changes the stage reload key to prevent accidental level reloads when typing");
    }

    private void OnSettingChanged(object sender, EventArgs e)
    {
        Log.Debug("OnSettingChanged");
        Application.runInBackground = e.AsConfigEntry<bool>().Value;
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
