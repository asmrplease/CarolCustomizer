using CarolCustomizer.Utils;
using BepInEx.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using CarolCustomizer.Models.Accessories;

namespace CarolCustomizer.Behaviors;
public class FavoritesManager : IDisposable
{
    ConfigFile config;
    ConfigEntry<string> favoritesConfig;
    //Action saveMethod;
    public HashSet<AccessoryDescriptor> favorites { get; private set; }

    public FavoritesManager(ConfigFile config)
    {
        this.config = config;
        this.favoritesConfig = config.Bind("Preferences", "Favorites", "", "List of favorited accessories.");
        LoadFavorites();
    }

    public void LoadFavorites()
    {
        //get the list of favorites from the save data
        string json = favoritesConfig.Value;
        favorites = JsonConvert.DeserializeObject<HashSet<AccessoryDescriptor>>(json);
        if (favorites is null) { Log.Warning("failed to load favoites"); }
        else Log.Debug($"Loaded {favorites.Count} favorites");
        favorites ??= new();
    }

    public void SaveFavorites()
    {
        //save the current list of favorites to text
        string json = JsonConvert.SerializeObject(favorites);
        favoritesConfig.Value = json;
        config.Save();
    }

    public bool IsInFavorites(AccessoryDescriptor descriptor)
    {
        return favorites.Contains(descriptor);
    }

    public void AddToFavorites(AccessoryDescriptor descriptor, bool autoSave = true)
    {
        favorites.Add(descriptor);
        if (autoSave) SaveFavorites();
    }

    public void RemoveFromFavorites(AccessoryDescriptor descriptor, bool autoSave = true)
    {
        if (IsInFavorites(descriptor)) favorites.Remove(descriptor);
        if (autoSave) SaveFavorites();
    }

    public void ToggleFavorite(AccessoryDescriptor descriptor, bool autoSave = true)
    {
        Log.Debug("FavoritesManager.ToggleFavorite()");
        if (IsInFavorites(descriptor)) RemoveFromFavorites(descriptor, autoSave);
        else AddToFavorites(descriptor, autoSave);
        Log.Debug("toggle done");
    }

    public void Dispose() => SaveFavorites();
}
