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
using UnityEngine.SceneManagement;
using System.Collections;

namespace CarolCustomizer.Behaviors.Recipes;
public class RecipeFileWatcher : IDisposable
{
    readonly FileSystemWatcher watcher;
    readonly Dictionary<string, Recipe> recipes = [];
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
        SceneManager.sceneUnloaded += (_) => RefreshSlow();
        MenuToggle.OnMenuToggle += MenuToggleHandle;
        OutfitAssetManager.OnOutfitSetLoaded += () => CCPlugin.CoroutineRunner.StartCoroutine(ReloadAll());
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
        if (visible) CCPlugin.CoroutineRunner.StartCoroutine(QuickRefresh());
    }
    
    IEnumerator QuickRefresh()
    {
        var paths = RecipeLoader.GetRecipeFilePaths();
        var missing = paths.Where(x => !recipes.ContainsKey(x));
        foreach (var path in missing)
        {
            var ext = Path.GetExtension(path).ToLower();
            if (ext != Constants.JsonFileExtension && ext != Constants.PngFileExtension) continue;
            OnRecipeFileCreated(new Recipe(path));
            yield return null;
        }
    }

    public IEnumerator ReloadAll()
    {
        recipes.Values.ForEach(x => OnRecipeDeleted?.Invoke(x));
        recipes.Clear();
        Log.Debug("sync load autosaves");
        var autosaves = RecipeLoader.GetRecipeFilePaths()
            .Where(x => x.Contains(Constants.AutoSave))
            .Select(x => new Recipe(x))
            .ForEach(OnRecipeFileCreated);
        Log.Debug("yield load recipes");
        var rest = RecipeLoader.GetRecipeFilePaths()
            .Where(x => !x.Contains(Constants.AutoSave))
            .ToList();
        foreach (var path in rest)
        {
            OnRecipeFileCreated(new Recipe(path));
            yield return null;
        }
    }

    public void RefreshSlow()
    {
        var slow = recipes
            .Where(x => x.Value.Error == Recipe.Status.SlowSource)
            .ToList();
        foreach (var entry in slow)
        {
            OnRecipeDeleted?.Invoke(entry.Value);
            recipes.Remove(entry.Key);
            OnRecipeFileCreated(new Recipe(entry.Key));
        }
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
