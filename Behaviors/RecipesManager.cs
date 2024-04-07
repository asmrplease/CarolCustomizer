using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CarolCustomizer.Models;
using CarolCustomizer.Utils;
using CarolCustomizer.Assets;

namespace CarolCustomizer.Behaviors;
public class RecipesManager : IDisposable
{
    FileSystemWatcher watcher;
    string path;

    public Action<Recipe> OnRecipeCreated;
    public Action<Recipe> OnRecipeDeleted;

    Dictionary<string, Recipe> recipes = new();

    public IEnumerable<Recipe> Recipes => recipes.Values;

    public RecipesManager(string path)
    {
        this.path = path;
        this.watcher = new FileSystemWatcher(path);

        this.watcher.Filter = $"*{Constants.RecipeExtension}";
        this.watcher.Created += HandleRecipeFileCreated;
        this.watcher.Deleted += HandleRecipeFileRemoved;
        this.watcher.Changed += HandleRecipeFileChanged;
        this.watcher.Renamed += HandleRecipeFileRenamed;

        this.watcher.EnableRaisingEvents = true;

        OutfitAssetManager.OnHaDSOutfitsLoaded += RefreshAll;
    }

    public void Dispose() => this.DisposeFields();

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
