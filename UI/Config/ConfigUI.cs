using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Config;
public class ConfigUI : MonoBehaviour
{
    const string DropdownAddress = "KeyCode Dropdown/Dropdown";
    public void Constructor()
    {
        var dropdown = transform.Find(DropdownAddress);
        if (!dropdown) Log.Warning("didn't find dropdown");
        var keycode = dropdown.gameObject.AddComponent<KeyCodeDropDown>();
        keycode.Constructor(Settings.HotKeys.keyboardMenuToggle);
    }
}
