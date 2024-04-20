using BepInEx.Configuration;
using CarolCustomizer.Assets;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace CarolCustomizer.UI.Config;

public class KeyCodeDropDown : MonoBehaviour
{
    const string LabelAddress = "Label";
    static readonly List<KeyCode> keys = Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>().ToList();

    Dropdown dropdown;
    Text label;
    ConfigEntry<KeyCode> keycodeConfig;

    public KeyCodeDropDown Constructor(ConfigEntry<KeyCode> keycodeConfig)
    {
        Log.Debug("ConfigUI.Constructor()");
        this.keycodeConfig = keycodeConfig;

        dropdown = GetComponent<Dropdown>();
        dropdown.onValueChanged.AddListener(OnDropdownChanged);

        label = transform.Find(LabelAddress).gameObject.GetComponent<Text>();
        label.text = keycodeConfig.ToString();

        foreach (var key in keys)
        {
            Log.Debug($"adding keycode: {key}");
            Dropdown.OptionData idk = new();
            idk.text = key.ToString();
            dropdown.options.Add(idk);
        }

        SetKeyCode(keycodeConfig.Value);
        return this;
    }

    private void OnDropdownChanged(int index)
    {
        var key = keys[index];  
        Log.Debug($"dropdown set to index: {index}, value {key}");
        keycodeConfig.Value = key;
    }

    public void SetKeyCode(KeyCode keyCode)
    {
        dropdown.value = keys.IndexOf(keyCode);
    }
}
