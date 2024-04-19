using BepInEx.Configuration;
using CarolCustomizer.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CarolCustomizer.Behaviors.Settings;
public class HotKeyConfig
{
    public readonly ConfigEntry<KeyCode> mouseMenuToggle;
    public readonly ConfigEntry<KeyCode> keyboardMenuToggle;
    public readonly ConfigEntry<PointerEventData.InputButton> mouseContextMenu;
    public KeyCode MenuToggleMouse => mouseMenuToggle.Value;
    public KeyCode MenuToggleKeyboard => keyboardMenuToggle.Value;
    public PointerEventData.InputButton ContextMenu => mouseContextMenu.Value;

    public HotKeyConfig(ConfigFile config)
    {
        mouseMenuToggle = config.Bind(
            Constants.Preferences, 
            "Menu Toggle (Mouse)", 
            KeyCode.Mouse3, 
            "Mouse shortcut for opening the accessory menu.");
        keyboardMenuToggle = config.Bind(
            Constants.Preferences, 
            "Menu Toggle (Keyboard)", 
            KeyCode.Keypad0, 
            "Keyboard shortcut for opening the accessory menu.");
        mouseContextMenu = config.Bind(
            Constants.Preferences, 
            "Context Menu Button", 
            PointerEventData.InputButton.Right, 
            "Which mouse button activates the context menu.");
    }
}
