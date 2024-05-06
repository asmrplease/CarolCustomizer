using System;
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

    public Action<Recipe> OnRecipeCreated;
    public Action<Recipe> OnRecipeDeleted;

    Dictionary<string, Recipe> recipes = new();

    public IEnumerable<Recipe> Recipes => recipes.Values;

    public Recipe GetRecipeByFilename(string name)
    {
        string address = RecipeSaver.RecipeFilenameToPath(name + Constants.RecipeExtension);
        Log.Debug($"address: {address}");
        recipes.TryGetValue(address, out Recipe recipe); return recipe;
    }

    public RecipesManager(string path)
    {
        Directory.CreateDirectory(path);
        watcher = new FileSystemWatcher(path);
        watcher.Filter = $"*{Constants.RecipeExtension}";
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
        foreach (var recipe in recipes.Values) { OnRecipeDeleted(recipe); }
        recipes.Clear();

        foreach (string existingPath in RecipeLoader.GetRecipeFilePaths())
        {
            OnRecipeFileCreated(new Recipe(existingPath));
        }
    }

    private void OnRecipeFileCreated(Recipe newRecipe)
    {
        Log.Debug($"OnRecipeFileCreated({newRecipe.Name})");
        recipes[newRecipe.Path] = newRecipe;
        Log.Debug($"OnRecipeCreated?.Invoke({newRecipe.Name})");
        OnRecipeCreated?.Invoke(newRecipe);
    }

    private void OnRecipeFileRemoved(Recipe removedRecipe)
    {
        Log.Debug($"OnRecipeFileRemoved({removedRecipe.Name})");
        recipes.Remove(removedRecipe.Path);
        OnRecipeDeleted?.Invoke(removedRecipe);
    }

    private void HandleRecipeFileCreated(object sender, FileSystemEventArgs e)
    {
        Log.Debug("HandleRecipeFileCreated");
        var newRecipe = new Recipe(e.FullPath);
        //Log.Debug($"recipe created: {newRecipe.Name}");
        OnRecipeFileCreated(newRecipe);
    }

    private void HandleRecipeFileChanged(object sender, FileSystemEventArgs e)
    {
        Log.Debug("HandleRecipeFileChanged");
        OnRecipeFileRemoved(recipes[e.FullPath]);
        OnRecipeFileCreated(new Recipe(e.FullPath));
    }

    private void HandleRecipeFileRenamed(object sender, RenamedEventArgs e)
    {
        Log.Debug("HandleRecipeFileRenamed");
        OnRecipeFileRemoved(recipes[e.OldName]);
        OnRecipeFileCreated(new Recipe(e.FullPath));
    }

    private void HandleRecipeFileRemoved(object sender, FileSystemEventArgs e)
    {
        Log.Debug("HandleRecipeFileRemoved");
        if (!recipes.ContainsKey(e.FullPath)) return;
        var removed = recipes[e.FullPath];
        OnRecipeFileRemoved(removed);
    }
}
