using BepInEx.Configuration;
using CarolCustomizer.Utils;

namespace CarolCustomizer.Behaviors.Settings;
public class PluginConfig
{
    public readonly ConfigEntry<string> menuSpeed;
    public float MenuSpeed => Constants.MenuSpeeds[menuSpeed.Value];
    public PluginConfig(ConfigFile config)
    {
        menuSpeed = config.Bind<string>(
            Constants.Preferences,
            "Menu Open/Close Speed",
            "Normal",
            "Adjusts the speed of the menu");
    }
}
