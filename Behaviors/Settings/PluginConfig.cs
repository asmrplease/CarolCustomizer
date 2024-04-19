using BepInEx.Configuration;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarolCustomizer.Behaviors.Settings;
public class PluginConfig
{
    ConfigFile config;
    ConfigEntry<float> menuSpeed;
    public float MenuSpeed => menuSpeed.Value;
    public PluginConfig(ConfigFile config)
    {
        this.config = config;
        menuSpeed = config.Bind<float>(
            Constants.Preferences,
            "Menu Open/Close Speed",
            1.0f,
            "Adjusts the speed of the menu");

    }
}
