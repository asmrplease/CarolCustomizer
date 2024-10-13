using System;
using System.Collections.Generic;
using System.IO;
using CarolCustomizer.Utils;
using CarolCustomizer.Assets;
using CarolCustomizer.Models.Recipes;
using UnityEngine;
using BepInEx;
using System.Linq;
using CarolCustomizer.UI.Main;

namespace CarolCustomizer.Behaviors.Recipes;
public class RecipeFileWatcher : IDisposable
{
    readonly FileSystemWatcher watcher;
    readonly Dictionary<string, Recipe> recipes = new();
    public List<Recipe> AllRecipes => recipes.Values.ToList();

    public event Action<Recipe> OnRecipeCreated;
    public event Action<Recipe> OnRecipeDeleted;

    public IEnumerable<Recipe> Recipes => recipes.Values;

    public Recipe GetRecipeByFilename(string name)
    {
        string address = RecipeSaver.RecipeFilenameToPath(name);
        Log.Debug($"address: {address}");
        recipes.TryGetValue(address, out Recipe recipe); 
        return recipe;
    }

    public RecipeFileWatcher(string path)
    {
        Directory.CreateDirectory(path);
        watcher = new(path)
        {
            IncludeSubdirectories = true
        };
        watcher.Created += HandleRecipeFileCreated;
        watcher.Deleted += HandleRecipeFileRemoved;
        watcher.Changed += HandleRecipeFileChanged;
        watcher.EnableRaisingEvents = true;

        MenuToggle.OnMenuToggle += MenuToggleHandle;

        OutfitAssetManager.OnOutfitSetLoaded += RefreshAll;
        Application.quitting += Dispose;
    }

    public void Dispose()
    {
        watcher.EnableRaisingEvents = false;
        watcher.Created -= HandleRecipeFileCreated;
        watcher.Deleted -= HandleRecipeFileRemoved;
        watcher.Changed -= HandleRecipeFileChanged;
        watcher.Dispose();

        MenuToggle.OnMenuToggle -= MenuToggleHandle;
    }

    void MenuToggleHandle(bool visible)
    {
        watcher.EnableRaisingEvents = visible;
    }

    public void RefreshAll()
    {
        recipes.Values.ForEach(x => OnRecipeDeleted?.Invoke(x));
        recipes.Clear();

        RecipeLoader
            .GetRecipeFilePaths()
            .ForEach(x => OnRecipeFileCreated(new Recipe(x)));
    }

    void OnRecipeFileCreated(Recipe newRecipe)
    {
        Log.Debug($"OnRecipeFileCreated({newRecipe.Name})");
        recipes[newRecipe.Path] = newRecipe;
        Log.Debug($"OnRecipeCreated?.Invoke({newRecipe.Name})");
        ThreadingHelper
            .Instance
            .StartSyncInvoke(() =>
                OnRecipeCreated?.Invoke(newRecipe));
    }

    void OnRecipeFileRemoved(Recipe removedRecipe)
    {
        Log.Debug($"OnRecipeFileRemoved({removedRecipe.Name})");
        recipes.Remove(removedRecipe.Path);
        ThreadingHelper
            .Instance
            .StartSyncInvoke(() => 
                OnRecipeDeleted?.Invoke(removedRecipe));
    }

    void HandleRecipeFileCreated(object source, FileSystemEventArgs e)
    {
        var ext = Path.GetExtension(e.FullPath).ToLower();
        if (ext != Constants.JsonFileExtension && ext != Constants.PngFileExtension) return;
        Log.Debug("HandleRecipeFileCreated");
        var newRecipe = new Recipe(e.FullPath);
        OnRecipeFileCreated(newRecipe);
    }

    void HandleRecipeFileChanged(object source, FileSystemEventArgs e)
    {
        var ext = Path.GetExtension(e.FullPath).ToLower();
        if (ext != Constants.JsonFileExtension && ext != Constants.PngFileExtension) return;
        Log.Debug("HandleRecipeFileChanged");
        OnRecipeFileRemoved(recipes[e.FullPath]);
        if (!File.Exists(e.FullPath)) return;

        OnRecipeFileCreated(new Recipe(e.FullPath));
    }
    void HandleRecipeFileRemoved(object source, FileSystemEventArgs e)
    {
        var ext = Path.GetExtension(e.FullPath).ToLower();
        if (ext != Constants.JsonFileExtension && ext != Constants.PngFileExtension) return;
        Log.Debug("HandleRecipeFileRemoved");
        if (!recipes.ContainsKey(e.FullPath)) return;
        var removed = recipes[e.FullPath];
        OnRecipeFileRemoved(removed);
    }
}
