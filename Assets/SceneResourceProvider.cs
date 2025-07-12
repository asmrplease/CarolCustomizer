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
    //we want a list of materials to load, grouped by the scene that they're sourced from
    //this list should be populated from the player's marked favorites, and the list of recipes
    //the recipe applier needs to report the list of objects to be loaded
    //once all objects have been queued, call the load method to sequentially load the requested assets
    //but then we either need separate queues for scenes/objects that we want to load now versus low 
    readonly static HashSet<MaterialDescriptor> lazyLoad = [];
    readonly static Dictionary<MaterialDescriptor, List<Action<MaterialDescriptor>>> batchLoad = [];
    readonly static HashSet<MaterialDescriptor> loaded = [];
    readonly static HashSet<string> loadedScenes = [];
    public static bool FakeLoad { get; private set; } = false;

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
        GetBatchResourcesFromScene(currentScene);
        foreach (var scene in batchLoadScenes) yield return BatchLoadMatsFromScene(scene);

        yield break;
    }

    static IEnumerator BatchLoadMatsFromScene(string scene)
    {
        Log.Debug("Starting Scene Load");
        FakeLoad = true;
        loadedScenes.Add(scene);
        yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        //GetLazyResourcesFromScene(scene);
        GetBatchResourcesFromScene(scene);
        yield return SceneManager.UnloadSceneAsync(scene);
        yield return Resources.UnloadUnusedAssets();
        loadedScenes.Remove(scene);
        FakeLoad = false;
        Log.Info("Scene Unload complete.");
    }

    static void GetBatchResourcesFromScene(string sceneName)
    {
        Log.Info($"GetBatchResourcesFromScene({sceneName}");
        var inThisScene = batchLoad
            .Where(x => x.Key.Source == sceneName)
            .Select(x => x.Key.Name.DeInstance())
            .ToList();
        Log.Info("Assets pending load:");
        inThisScene.ForEach(Log.Info);
        int pendingCount = inThisScene.Count;
        var foundList = Resources.FindObjectsOfTypeAll<Material>()
            .Where(x => inThisScene.Contains(x.name))
            .Select(x => new MaterialDescriptor(x, sceneName, MaterialDescriptor.SourceType.World));
        int loadedCount = 0;
        foreach (var found in foundList) 
        {
            if (!batchLoad.TryGetValue(found, out var callbacks)) { continue; }

            found.referenceMaterial.hideFlags = HideFlags.HideAndDontSave;
            loaded.Add(found);
            batchLoad.Remove(found);
            Log.Info($"Calling back {callbacks.Count()} method(s) for {found.Name}");
            callbacks.ForEach(callback => callback?.Invoke(found));
            loadedCount++;
        }
        Log.Info($"Loaded {loadedCount} of {pendingCount} materials");
    }

    internal static void AddToLazyLoad(MaterialDescriptor material)
    {
        if (loaded.Contains(material)) { Log.Info("Material already loaded"); return; }

        lazyLoad.Add(material);
    }
    /// <summary>
    /// Quickly store a material 
    /// </summary>
    /// <param name="material"></param>
    internal static void Cache(MaterialDescriptor material)
    {
        if (!material.referenceMaterial) { Log.Warning($"Tried to cache MaterialDescriptor {material.Name}, but there was no actual material attached."); return; }

        loaded.Add(material);
    }

    internal static IEnumerator LoadMaterialAndThen(MaterialDescriptor requestedMat, Action<MaterialDescriptor> closure)
    {
        var existing = loaded.FirstOrDefault(x => x.Equals(requestedMat));
        if (existing is null)
        {
            AddToLazyLoad(requestedMat);
            yield return LazyLoadMatsFromScene(requestedMat.Source);
            existing = loaded.FirstOrDefault(x => x.Equals(requestedMat));
        }
        if (existing is null) { Log.Warning($"failed to get {requestedMat.Name} from scene"); yield break; }
        if (!existing.referenceMaterial) { Log.Warning($"no material found in material descriptor for {requestedMat.Name}"); yield break; }

        Log.Info($"Applying {existing.Name}");
        closure?.Invoke(existing);
        yield break;
    }

    static IEnumerator LazyLoadMatsFromScene(string scene)
    {
        if (loadedScenes.Contains(scene) || SceneManager.GetActiveScene().name == scene) 
        {
            yield return new WaitUntil(() => FakeLoad is false);
            GetLazyResourcesFromScene(scene);
            yield break; 
        }
        
        Log.Debug("Starting Scene Load");
        FakeLoad = true;
        loadedScenes.Add(scene);
        yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        GetLazyResourcesFromScene(scene);
        yield return SceneManager.UnloadSceneAsync(scene);
        yield return Resources.UnloadUnusedAssets();
        loadedScenes.Remove(scene);
        FakeLoad = false;
        Log.Info("Scene Unload complete.");
    }

    static void GetLazyResourcesFromScene(string sceneName)
    {
        var inThisScene = lazyLoad
            .Where(x => x.Source == sceneName)
            .Select(x => x.Name.DeInstance())
            .ToList();
        Log.Info("Assets pending load:");
        inThisScene.ForEach(Log.Info);
        int pendingCount = inThisScene.Count;
        int loadedCount = Resources.FindObjectsOfTypeAll<Material>()
            .Where(x => inThisScene.Contains(x.name))
            .Select(x => new MaterialDescriptor(x, sceneName, MaterialDescriptor.SourceType.World))
            .ForEach(x =>
            {
                x.referenceMaterial.hideFlags = HideFlags.HideAndDontSave;
                loaded.Add(x);
                lazyLoad.Remove(x);
            })
            .Count();
        Log.Info($"Loaded {loadedCount} of {pendingCount} materials");
    }

    internal enum SceneAssetStatus
    {
        Ready,
        RequiresSceneLoad,
        RequiresAssetBundle,
    }

    async Awaitable<int> Test()
    {
        var idk = AssetBundle.LoadFromFileAsync("");
        var bundle = idk.assetBundle;
        var wtf = SceneManager.LoadSceneAsync("");
        List<AsyncOperation> ops = [idk, wtf];
        foreach (var op in ops)
        {
            await op;
        }
        return 0;
    }

    async Awaitable idk()
    {
        var idk = await Test();
    }
}

