using BepInEx.Configuration;
using CarolCustomizer.Utils;

namespace CarolCustomizer.Behaviors.Settings;
public class PluginConfig
{
    public readonly ConfigEntry<string> menuSpeed;
    public readonly ConfigEntry<bool> customMPBots;
    public readonly ConfigEntry<bool> customCampaignBots;
    public float MenuSpeed => Constants.MenuSpeeds[menuSpeed.Value];
    public PluginConfig(ConfigFile config)
    {
        menuSpeed = config.Bind<string>(
            Constants.Preferences,
            "Menu Open/Close Speed",
            "Normal",
            "Adjusts the speed of the menu");

        customMPBots = config.Bind<bool>(
            Constants.Preferences,
            "Customize Multiplayer Bots",
            true,
            "Enable loading recipes on multiplayer bots"
            );

        customCampaignBots = config.Bind<bool>(
            Constants.Preferences,
            "Customize Campaign Bots",
            true,
            "Enable loading recipes on campaign bots"
            );
    }
}
