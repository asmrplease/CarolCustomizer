using BepInEx.Configuration;

namespace CarolCustomizer.Behaviors.Settings;
public static class Settings 
{
    static public ConfigFile Config { get; private set; }

    static public HotKeyConfig HotKeys { get; private set; }
    static public FavoritesManager Favorites { get; private set; }
    static public GameSettings Game { get; private set; }
    static public PluginConfig Plugin { get; private set; }
    static public void Constructor(ConfigFile config)
    {
        Settings.Config     = config;
        Settings.HotKeys    = new(config);
        Settings.Favorites  = new(config);
        Settings.Game       = new(config);
        Settings.Plugin     = new(config);
    }

    static public void Dispose()
    {
        Config.Save();
        Favorites.Dispose();
        Game.Dispose();
    }
}
