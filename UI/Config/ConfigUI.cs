using BepInEx.Configuration;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.UI.Main;
using CarolCustomizer.Utils;
using System.ComponentModel;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Config;
public class ConfigUI : MonoBehaviour
{
    #region Transform Addresses
    const string KeyboardToggleAddress = "Menu Toggle Keyboard Key/Dropdown";
    const string MouseToggleAddress = "Menu Toggle Mouse Key/Dropdown";
    const string ReloadStageAddress = "Reload Stage Replacement Key/Dropdown";

    const string LogFolderButtonAddress = "OpenLogFolder";
    const string ClearFavoritesButtonAddress = "ClearFavorites";

    const string RunInBackgroundToggleAddress = "RunInBackground/Toggle";
    const string MultiplayerBotToggleAddress = "CustomMPBots/Toggle";
    const string CampaignBotToggleAddress = "CustomCampaignBots/Toggle";
    const string CustomShezaraToggleAddress = "CustomShezara/Toggle";

    const string RightMouseButtonToggle = "MouseButton/Toggles/Right";
    const string MenuSpeedToggleGroupAddress = "Menu Speed";
    #endregion
    #region Dependencies
    MessageDialogue dialoge;
    #endregion
    #region Setup
    public void Constructor(MessageDialogue dialogue)
    {
        this.dialoge = dialogue;

        SetupButton(LogFolderButtonAddress, OpenLogFolder);
        SetupButton(ClearFavoritesButtonAddress, ConfirmClearFavorites);

        SetupToggle(RunInBackgroundToggleAddress, Settings.Game.RunInBackgroundCE);
        SetupToggle(CampaignBotToggleAddress, Settings.Plugin.customCampaignBots);
        SetupToggle(MultiplayerBotToggleAddress, Settings.Plugin.customMPBots);
        
        SetupToggle(CustomShezaraToggleAddress, Settings.Plugin.customShezara);
        Settings.Plugin.customShezara.SettingChanged += OnShezaraEnabledChange;

        SetupDropdown(KeyboardToggleAddress, Settings.HotKeys.keyboardMenuToggle);
        SetupDropdown(MouseToggleAddress, Settings.HotKeys.mouseMenuToggle);
        SetupDropdown(ReloadStageAddress, Settings.Game.Reload);
        
        var toggles = transform
            .Find(MenuSpeedToggleGroupAddress)
            .GetComponentsInChildren<Toggle>(true);
        foreach (Toggle toggle in toggles)
        {
            toggle.onValueChanged.AddListener(OnMenuSpeedToggleChanged);
            if (toggle.name != Settings.Plugin.menuSpeed.Value) continue;
            toggle.SetIsOnWithoutNotify(true);
            break;
        }

        var RMBToggle = transform
            .Find(RightMouseButtonToggle)
            .GetComponent<Toggle>();
        RMBToggle
            .onValueChanged
            .AddListener(OnContextToggleChanged);
        RMBToggle
            .SetIsOnWithoutNotify(
            Settings.HotKeys.mouseContextMenu.Value == PointerEventData.InputButton.Right);
    }

    private void SetupDropdown(string address, ConfigEntry<KeyCode> setting)
    {
        transform
            .Find(address)
            .gameObject
            .AddComponent<KeyCodeDropDown>()
            .Constructor(setting);
    }

    private void SetupToggle(string address, ConfigEntry<bool> setting)
    {
        var toggle = transform
            .Find(address)
            .GetComponent<Toggle>();
        toggle
            .onValueChanged
            .AddListener
            ((x) => setting.Value = x);
        toggle
            .SetIsOnWithoutNotify
            (setting.Value);
    }

    private void SetupButton(string address, UnityAction callback)
    {
        transform
            .Find(address)
            .GetComponent<Button>()
            .onClick
            .AddListener(callback);
    }
    #endregion
    #region Callbacks
    private void OnShezaraEnabledChange(object sender, System.EventArgs e)
    {
        if (SceneManager.GetActiveScene().name == Constants.MenuSceneName) return;

        Log.Debug("Showing player setting popup");
        dialoge.Show(
            "This setting change will take effect after the level reloads.",
            cancelText: "Okay.");
    }

    private void OnMenuSpeedToggleChanged(bool state)
    {
        Log.Debug("OnMenuSpeedToggleChanged");
        string activeToggle = transform
            .Find(MenuSpeedToggleGroupAddress)
            .GetComponent<ToggleGroup>()
            .GetFirstActiveToggle()?
            .name;
        Settings.Plugin.menuSpeed.Value = activeToggle;
    }

    private void OnContextToggleChanged(bool state)
    {
        Settings.HotKeys.mouseContextMenu.Value = 
            state?
            PointerEventData.InputButton.Right:
            PointerEventData.InputButton.Middle;
    }

    private void ConfirmClearFavorites()
    {
        dialoge.Show(
            "Are you sure you want to remove everything from the favorites list?",
            "Yes.", Settings.Favorites.ResetFavorites,
            "Cancel!", null);
    }

    private void OpenLogFolder()
    {
        //string argument = @"/select, " + "\"" + Constants.LogFileName + "\""; //TODO get highlighting the file working?
        try 
        { 
            if (!Input.GetKey(KeyCode.LeftControl)) Process.Start(Constants.BepInExFolderPath);
            else Process.Start(Application.persistentDataPath);
        }
        catch (Win32Exception e) { Log.Warning(e.Message); }
    }

    private void OpenLogFile()
    {
        try { Process.Start(Constants.LogFilePath); }
        catch (Win32Exception e) { Log.Warning(e.Message); }
    }
    #endregion
}
