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
        this.watcher.Created += OnRecipeFileCreated;
        this.watcher.Deleted += OnRecipeFileRemoved;
        this.watcher.Changed += OnRecipeFileChanged;
        this.watcher.Renamed += OnRecipeFileRenamed;

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
        recipes[newRecipe.Path] = newRecipe;
        OnRecipeCreated?.Invoke(newRecipe);
    }

    private void OnRecipeFileRemoved(Recipe removedRecipe)
    {
        recipes.Remove(removedRecipe.Path);
        OnRecipeDeleted?.Invoke(removedRecipe);
    }

    private void OnRecipeFileCreated(object sender, FileSystemEventArgs e)
    {
        Log.Debug("OnRecipeFileCreated");
        var newRecipe = new Recipe(e.FullPath);
        OnRecipeFileCreated(newRecipe);
    }

    private void OnRecipeFileChanged(object sender, FileSystemEventArgs e)
    {
        OnRecipeFileRemoved(recipes[e.FullPath]);
        OnRecipeFileCreated(new Recipe(e.FullPath));
    }

    private void OnRecipeFileRenamed(object sender, RenamedEventArgs e)
    {
        OnRecipeFileRemoved(recipes[e.OldName]);
        OnRecipeFileCreated(new Recipe(e.FullPath));
    }

    private void OnRecipeFileRemoved(object sender, FileSystemEventArgs e)
    {
        if (!recipes.ContainsKey(e.FullPath)) return;
        var removed = recipes[e.FullPath];
        OnRecipeFileRemoved(removed);
    }

}
