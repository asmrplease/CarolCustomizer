using System;
using System.Collections.Generic;
using System.IO;
using CarolCustomizer.Utils;
using CarolCustomizer.Models.Recipes;
using UnityEngine;
using BepInEx;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;
using CarolCustomizer.UI.Legacy.Main;

namespace CarolCustomizer.Behaviors.Recipes;
public class RecipeFileWatcher : IDisposable
{
    readonly FileSystemWatcher watcher;
    readonly Dictionary<string, RecipeFile> recipes = [];
    public List<RecipeFile> AllRecipes => recipes.Values.ToList();

    public static event Action<RecipeFile> OnRecipeCreated;
    public static event Action<RecipeFile> OnRecipeDeleted;

    public IEnumerable<RecipeFile> Recipes => recipes.Values;

    public RecipeFile GetRecipeByFilename(string name)
    {
        string address = RecipeSaver.RecipeFilenameToPath(name);
        Log.Debug($"address: {address}");
        recipes.TryGetValue(address, out RecipeFile recipe); 
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
        //OutfitAssetManager.OnOutfitSetLoaded += () => CCPlugin.CoroutineRunner.StartCoroutine(ReloadAll());
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
            OnRecipeFileCreated(new RecipeFile(path));
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
            .Select(x => new RecipeFile(x))
            .ForEach(OnRecipeFileCreated);
        Log.Debug("yield load recipes");
        var rest = RecipeLoader.GetRecipeFilePaths()
            .Where(x => !x.Contains(Constants.AutoSave))
            .ToList();
        foreach (var path in rest)
        {
            OnRecipeFileCreated(new RecipeFile(path));
            yield return null;
        }
    }

    public void RefreshSlow()
    {
        var slow = recipes
            .Where(x => x.Value.Error == RecipeFile.Status.SlowSource)
            .ToList();
        foreach (var entry in slow)
        {
            OnRecipeDeleted?.Invoke(entry.Value);
            recipes.Remove(entry.Key);
            OnRecipeFileCreated(new RecipeFile(entry.Key));
        }
    }

    void OnRecipeFileCreated(RecipeFile newRecipe)
    {
        Log.Debug($"OnRecipeFileCreated({newRecipe.Name})");
        recipes[newRecipe.Path] = newRecipe;
        Log.Debug($"OnRecipeCreated?.Invoke({newRecipe.Name})");
        ThreadingHelper
            .Instance
            .StartSyncInvoke(() =>
                OnRecipeCreated?.Invoke(newRecipe));
    }

    void OnRecipeFileRemoved(RecipeFile removedRecipe)
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
        var newRecipe = new RecipeFile(e.FullPath);
        OnRecipeFileCreated(newRecipe);
    }

    void HandleRecipeFileChanged(object source, FileSystemEventArgs e)
    {
        var ext = Path.GetExtension(e.FullPath).ToLower();
        if (ext != Constants.JsonFileExtension && ext != Constants.PngFileExtension) return;
        Log.Debug("HandleRecipeFileChanged");
        OnRecipeFileRemoved(recipes[e.FullPath]);
        if (!File.Exists(e.FullPath)) return;

        OnRecipeFileCreated(new RecipeFile(e.FullPath));
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
