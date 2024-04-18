using BepInEx.Configuration;


namespace CarolCustomizer.Behaviors.Settings;
public static class Settings 
{
    static ConfigFile config;

    static public HotKeyConfig HotKeys { get; private set; }
    static public FavoritesManager Favorites { get; private set; }
    static public GameSettings Game { get; private set; }
    static public void Constructor(ConfigFile config)
    {
        Settings.config = config;
        Settings.HotKeys = new(config);
        Settings.Favorites = new(config);
        Settings.Game = new(config);
    }

    static public void Dispose()
    {
        config.Save();
        Favorites.Dispose();
        Game.Dispose();
    }
}
