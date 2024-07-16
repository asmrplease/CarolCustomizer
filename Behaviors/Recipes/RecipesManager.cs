﻿using System;
using System.Collections.Generic;
using System.IO;
using CarolCustomizer.Utils;
using CarolCustomizer.Assets;
using CarolCustomizer.Models.Recipes;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Recipes;
public class RecipesManager : IDisposable
{
    FileSystemWatcher watcher;
    Dictionary<string, Recipe> recipes = new();

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

    public RecipesManager(string path)
    {
        Directory.CreateDirectory(path);
        watcher = new FileSystemWatcher(path);
        //watcher.Filter = $"*{Constants.RecipeExtension}";
        watcher.IncludeSubdirectories = true;
        watcher.Created += HandleRecipeFileCreated;
        watcher.Deleted += HandleRecipeFileRemoved;
        watcher.Changed += HandleRecipeFileChanged;
        watcher.Renamed += HandleRecipeFileRenamed;
        watcher.EnableRaisingEvents = true;

        OutfitAssetManager.OnOutfitSetLoaded += RefreshAll;
        Application.quitting += Dispose;
    }

    public void Dispose()
    {
        watcher.EnableRaisingEvents = false;
        watcher.Dispose();
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
        OnRecipeCreated?.Invoke(newRecipe);
    }

    void OnRecipeFileRemoved(Recipe removedRecipe)
    {
        Log.Debug($"OnRecipeFileRemoved({removedRecipe.Name})");
        recipes.Remove(removedRecipe.Path);
        OnRecipeDeleted?.Invoke(removedRecipe);
    }

    void HandleRecipeFileCreated(object sender, FileSystemEventArgs e)
    {
        var ext = Path.GetExtension(e.FullPath).ToLower();
        if (ext != Constants.RecipeExtension && ext != Constants.RecipeImageExtension) return;
        Log.Debug("HandleRecipeFileCreated");
        var newRecipe = new Recipe(e.FullPath);
        //Log.Debug($"recipe created: {newRecipe.Name}");
        OnRecipeFileCreated(newRecipe);
    }

    void HandleRecipeFileChanged(object sender, FileSystemEventArgs e)
    {
        var ext = Path.GetExtension(e.FullPath).ToLower();
        if (ext != Constants.RecipeExtension && ext != Constants.RecipeImageExtension) return;
        Log.Debug("HandleRecipeFileChanged");
        OnRecipeFileRemoved(recipes[e.FullPath]);
        OnRecipeFileCreated(new Recipe(e.FullPath));
    }

    void HandleRecipeFileRenamed(object sender, RenamedEventArgs e)
    {
        var ext = Path.GetExtension(e.FullPath).ToLower();
        if (ext != Constants.RecipeExtension && ext != Constants.RecipeImageExtension) return;
        Log.Debug("HandleRecipeFileRenamed");
        OnRecipeFileRemoved(recipes[e.OldName]);
        OnRecipeFileCreated(new Recipe(e.FullPath));
    }

    void HandleRecipeFileRemoved(object sender, FileSystemEventArgs e)
    {
        var ext = Path.GetExtension(e.FullPath).ToLower();
        if (ext != Constants.RecipeExtension && ext != Constants.RecipeImageExtension) return;
        Log.Debug("HandleRecipeFileRemoved");
        if (!recipes.ContainsKey(e.FullPath)) return;
        var removed = recipes[e.FullPath];
        OnRecipeFileRemoved(removed);
    }
}
