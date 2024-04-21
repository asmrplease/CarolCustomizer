using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.UI.Main;
using CarolCustomizer.Utils;
using System.ComponentModel;
using System.Diagnostics;
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
    const string MenuSpeedToggleGroupAddress = "Menu Speed";
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
        try { Process.Start(Constants.BepInExFolderPath); }
        catch (Win32Exception e) { Log.Warning(e.Message); }
    }

    private void OpenLogFile()
    {
        try { Process.Start(Constants.LogFilePath); }
        catch (Win32Exception e) { Log.Warning(e.Message); }
    }
}
