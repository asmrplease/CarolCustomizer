using CarolCustomizer.Utils;
using BepInEx.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using CarolCustomizer.Models.Accessories;
using static ModelData;

namespace CarolCustomizer.Behaviors.Settings;
public class FavoritesManager : IDisposable
{
    ConfigFile config;
    ConfigEntry<string> favoritesConfig;
    public Action OnFavoritesCleared;
    public HashSet<AccessoryDescriptor> favorites { get; private set; }

    public FavoritesManager(ConfigFile config)
    {
        this.config = config;
        favoritesConfig = config.Bind("Preferences", "Favorites", "", "List of favorited accessories.");
        LoadFavorites();
    }

    public void LoadFavorites()
    {
        string json = favoritesConfig.Value;
        favorites = JsonConvert.DeserializeObject<HashSet<AccessoryDescriptor>>(json);
        if (favorites is null) { Log.Warning("failed to load favoites"); }
        else Log.Debug($"Loaded {favorites.Count} favorites");
        favorites ??= new();
    }

    public void SaveFavorites()
    {
        string json = JsonConvert.SerializeObject(favorites);
        favoritesConfig.Value = json;
        config.Save();
    }

    public void ResetFavorites()
    {
        OnFavoritesCleared?.Invoke();
        favorites.Clear();
        SaveFavorites();
    }

    public bool IsInFavorites(AccessoryDescriptor descriptor) => favorites.Contains(descriptor);

    public void AddToFavorites(AccessoryDescriptor descriptor, bool autoSave = true)
    {
        favorites.Add(new AccessoryDescriptor(descriptor));
        if (autoSave) SaveFavorites();
    }

    public void RemoveFromFavorites(AccessoryDescriptor descriptor, bool autoSave = true)
    {
        if (IsInFavorites(descriptor)) favorites.Remove(descriptor);
        if (autoSave) SaveFavorites();
    }

    public void ToggleFavorite(AccessoryDescriptor descriptor, bool autoSave = true)
    {
        if (IsInFavorites(descriptor)) RemoveFromFavorites(descriptor, autoSave);
        else AddToFavorites(descriptor, autoSave);
    }

    public void Dispose() => SaveFavorites();
}
