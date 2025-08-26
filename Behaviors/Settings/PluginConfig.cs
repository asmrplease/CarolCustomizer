using BepInEx.Configuration;
using CarolCustomizer.Utils;
using System;
using System.Linq;
using System.Collections.Generic;
using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Carol;

namespace CarolCustomizer.Behaviors.Settings;
public class PluginConfig
{
    public readonly ConfigEntry<string> menuSpeed;
    public readonly ConfigEntry<bool> customMPBots;
    public readonly ConfigEntry<bool> customCampaignBots;
    public readonly ConfigEntry<bool> customAllNPCs;
    public readonly Dictionary<NPC, (ConfigEntry<bool> enable, ConfigEntry<string> recipe)> customNPCs;
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
            "Enable loading recipes on multiplayer bots");
        customCampaignBots = config.Bind<bool>(
            Constants.Preferences,
            "Customize Campaign Bots",
            true,
            "Enable loading recipes on campaign bots");
        customNPCs = NPCManager.NPCTypes()
            .Select(type => (type, NPCBinding(type, config)))
            .ToDictionary(x => x.type, x => x.Item2);
    }

    (ConfigEntry<bool>, ConfigEntry<string>) NPCBinding(NPC type, ConfigFile file)
    {
        string npcName = Enum.GetName(typeof(NPC), type);
        var enabled = file.Bind<bool>(
            Constants.Preferences,
            $"Customize {npcName}",
            false,
            $"Enable load a custom outfit on {npcName}");
        var recipe = file.Bind<string>(
            Constants.Preferences,
            $"{npcName} file name",
            npcName,
            $"Determines which recipe {npcName} is dressed with when enabled.");
        return (enabled, recipe);
    }
}
