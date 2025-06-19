using BepInEx.Configuration;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
        this.keycodeConfig = keycodeConfig;

        dropdown = GetComponent<Dropdown>();
        dropdown.onValueChanged.AddListener(OnDropdownChanged);

        label = transform.Find(LabelAddress).gameObject.GetComponent<Text>();
        label.text = keycodeConfig.ToString();

        foreach (var key in keys)
        {
            Dropdown.OptionData entry = new();
            entry.text = key.ToString();
            dropdown.options.Add(entry);
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
