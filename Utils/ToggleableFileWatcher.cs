using CarolCustomizer.Models.Recipes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CarolCustomizer.Utils;
public class ToggleableFileWatcher<T> : IDisposable
{
    readonly FileSystemWatcher watcher;
    readonly Dictionary<string, T> data = [];
    public string[] extensions;
    public event Action<T> OnCreated;
    public event Action<T> OnDestroyed;
    public Func<string, T> Build;

    public ToggleableFileWatcher(string path)
    {
        Directory.CreateDirectory(path);
        watcher = new FileSystemWatcher(path);
        watcher.Created += OnFileCreated;
        watcher.Changed += OnFileChanged;
        watcher.Deleted += OnFileRemoved; 
    }

    void HandleMenuToggle(bool visible)
    {
        watcher.EnableRaisingEvents = visible;
    }

    void FullRefresh()
    {
        //unload all 
        //reload all
    }

    void QuickRefresh()
    {

        //look for files that aren't loaded yet
        //call OnFileCreated foreach
        //how do we detect files that changed since the menu last opened?
    }

    public void Dispose()
    {
        watcher.EnableRaisingEvents = false;
        watcher.Dispose();
    }

    bool Filter(string path)
    {
        var ext = Path.GetExtension(path).ToLower();
        if (extensions.Contains(ext)) return false;
        if (MiscExtensions.DirectoryHidden(path)) return false;
        return true;
    }

    void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        var path = e.FullPath;
        if (!Filter(path)) return;
        if (data.ContainsKey(path)) { Log.Warning($"Tried to load {path} but it is already loaded."); return; }

        try { data[path] = Build.Invoke(path); }
        catch (Exception ex) { Log.Error($"Exception occured loading {path}: {ex.Message}"); return; }

        try { OnCreated?.Invoke(data[path]); }
        catch (Exception ex) { Log.Error($"Exception occured invoking file load handler: {ex.Message}"); }
    }

    void OnFileRemoved(object sender, FileSystemEventArgs e) 
    {
        var path = e.FullPath;
        if (!Filter(path)) return;

        try { OnDestroyed?.Invoke(data[path]); }
        catch (Exception ex) { Log.Error($"Exception occured invoking file unload handler: {ex.Message}"); }
        finally { data.Remove(path); }
    }
    void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        OnFileRemoved(sender, e);
        OnFileCreated(sender, e);
    }
}


internal class RecipeWatcher
{
    public static ToggleableFileWatcher<RecipeFile> Instance;

    public RecipeWatcher()
    {
        Instance = new ToggleableFileWatcher<RecipeFile>(Constants.RecipeFolderPath)
        {
            Build = (x) => new RecipeFile(x),
            extensions = [".png", ".json"],
        };
    }
}