using BepInEx.Configuration;
using CarolCustomizer.Utils;

namespace CarolCustomizer.Behaviors.Settings;
public class PluginConfig
{
    ConfigEntry<float> menuSpeed;
    public float MenuSpeed => menuSpeed.Value;
    public PluginConfig(ConfigFile config)
    {
        menuSpeed = config.Bind<float>(
            Constants.Preferences,
            "Menu Open/Close Speed",
            1.0f,
            "Adjusts the speed of the menu");
    }
}
