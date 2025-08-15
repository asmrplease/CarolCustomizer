using CarolCustomizer.Models.Materials;
using CarolCustomizer.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CarolCustomizer.Assets;
internal class SceneResourceProvider
{
    readonly static HashSet<MaterialDescriptor> lazyLoad = [];
    readonly static Dictionary<MaterialDescriptor, List<Action<MaterialDescriptor>>> batchLoad = [];
    readonly static HashSet<MaterialDescriptor> loaded = [];
    readonly static HashSet<string> loadedScenes = [];

    internal static event Action<MaterialDescriptor> OnMaterialLoaded;
    public static bool Loading { get; private set; } = false;

    internal static IEnumerable<string> CheckMaterialsReady(IEnumerable<MaterialDescriptor> materials)
    {
        return materials
            .Where(x => !loaded.Contains(x))
            .Select(x => x.Source)
            .Distinct();
    }

    internal static void SetCallback()
    {
        SceneManager.sceneLoaded += HandleNaturalSceneLoad;
    }

    static void HandleNaturalSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) return;

        GetResourcesFromScene(scene.name);
    }

    internal static IEnumerator BatchQueueAndThen(MaterialDescriptor request, Action<MaterialDescriptor> closure)
    {
        if (loaded.TryGetValue(request, out var existing)) { closure?.Invoke(existing); yield break; }
        if (!batchLoad.TryGetValue(request, out var actions)) actions = [];
        actions.Add(closure);
        batchLoad[request] = actions;//i forget if we have to do this or not but it doesn't hurt so we do it.
        yield break;
    }

    internal static IEnumerator BatchLoad()
    {
        Log.Info("BatchLoad()");
        if (Loading) { Log.Warning("Tried to BatchLoad() while already loading a scene."); yield break; }
    
        var reference = CCPlugin.uiInstance.loadingIndicator.NotifyLoadingStart();
        var currentScene = SceneManager.GetActiveScene().name;
        var batchLoadScenes = batchLoad
            .Select(x => x.Key.Source)
            .Distinct()
            .Where(x => currentScene != x)
            .ToList();  
        lazyLoad
            .Where(x => batchLoadScenes.Contains(x.Source))
            .ForEach(x => batchLoad.TryAdd(x, []))
            .ToList()
            .ForEach(x => lazyLoad.Remove(x));
        Log.Info("Loading the folling scenes:");
        batchLoadScenes.ForEach(Log.Info);
        GetResourcesFromScene(currentScene);
        foreach (var scene in batchLoadScenes) yield return BatchLoadMatsFromScene(scene);
        CCPlugin.uiInstance.loadingIndicator.NotifyLoadingComplete(reference);
        Loading = false;
        yield break;
    }

    static IEnumerator BatchLoadMatsFromScene(string scene)
    {
        Log.Debug("Starting Scene Load");
        
        loadedScenes.Add(scene);
        yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        GetResourcesFromScene(scene);
        yield return SceneManager.UnloadSceneAsync(scene);
        yield return Resources.UnloadUnusedAssets();
        loadedScenes.Remove(scene);
        Log.Info("Scene Unload complete.");
    }

    static void GetResourcesFromScene(string sceneName)
    {
        Log.Info($"GetBatchResourcesFromScene({sceneName}");
        var batchInScene = batchLoad
            .Where(x => x.Key.Source == sceneName)
            .Select(x => x.Key.Name.DeInstance())
            .ToList();
        var lazyInScene = lazyLoad
            .Where(x => x.Source == sceneName)
            .Select(x => x.Name.DeInstance())
            .ToList();
        var inThisScene = batchInScene
            .Concat(lazyInScene)
            .Distinct()
            .ToList();
        Log.Info("Assets pending load:");
        inThisScene.ForEach(Log.Info);
        int pendingCount = inThisScene.Count;
        var foundList = Resources.FindObjectsOfTypeAll<Material>()
            .Where(x => x && inThisScene.Contains(x.name))
            .Select(x => new MaterialDescriptor(x, sceneName, MaterialDescriptor.SourceType.World));
        int loadedCount = 0;
        foreach (var found in foundList) 
        {
            batchLoad.TryGetValue(found, out var callbacks);
            found.referenceMaterial.hideFlags = HideFlags.HideAndDontSave;
            loaded.Add(found);
            batchLoad.Remove(found);
            lazyLoad.Remove(found);
            OnMaterialLoaded?.Invoke(found);
            loadedCount++;
            if (callbacks is null) continue;

            Log.Info($"Calling back {callbacks.Count()} method(s) for {found.Name}");
            callbacks.ForEach(callback => callback?.Invoke(found));
        }
        Log.Info($"Loaded {loadedCount} of {pendingCount} materials");
    }

    internal static void AddToLazyLoad(MaterialDescriptor material)
    {
        if (loaded.Contains(material)) { Log.Debug("Material already loaded"); return; }

        lazyLoad.Add(material);
        OnMaterialLoaded?.Invoke(material);
    }
    /// <summary>
    /// Quickly store a material 
    /// </summary>
    /// <param name="material"></param>
    internal static void Cache(MaterialDescriptor material)
    {
        if (!material.referenceMaterial) { Log.Warning($"Tried to cache MaterialDescriptor {material.Name}, but there was no actual material attached."); return; }

        loaded.Add(material);
        OnMaterialLoaded?.Invoke(material);
    }
}

