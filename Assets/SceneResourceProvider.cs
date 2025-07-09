using CarolCustomizer.Behaviors.Recipes;
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
    readonly static HashSet<MaterialDescriptor> pendingLoad = [];
    readonly static HashSet<MaterialDescriptor> loaded = [];
    readonly static HashSet<string> loadedScenes = [];

    readonly static Dictionary<string, HashSet<MaterialDescriptor>> matsByScene = [];
    public static bool FakeLoad { get; private set; } = false;

    internal static void AddToPending(MaterialDescriptor material)
    {
        if (loaded.Contains(material)) { Log.Info("Material already loaded"); return; }

        pendingLoad.Add(material);
    }

    internal static void Cache(MaterialDescriptor material)
    {
        loaded.Add(material);
    }

    internal static IEnumerator LazyApplyMaterial(MaterialDescriptor requestedMat, Action<MaterialDescriptor> closure)
    {
        var existing = loaded.FirstOrDefault(x => x.Equals(requestedMat));
        if (existing is null)
        {
            AddToPending(requestedMat);
            yield return LoadMatsFromSceneAsync(requestedMat.Source);
            existing = loaded.FirstOrDefault(x => x.Equals(requestedMat));
        }
        if (existing is null) { Log.Warning($"failed to get {requestedMat.Name} from scene"); yield break; }
        if (!existing.referenceMaterial) { Log.Warning($"no material found in material descriptor for {requestedMat.Name}"); yield break; }

        Log.Info($"Applying {existing.Name}");
        closure?.Invoke(existing);
        yield break;
    }

    static IEnumerator LoadMatsFromSceneAsync(string scene)
    {
        if (loadedScenes.Contains(scene) || SceneManager.GetActiveScene().name == scene) 
        {
            yield return new WaitUntil(() => FakeLoad is false);
            GetResourcesFromScene(scene);
            yield break; 
        }
        
        Log.Debug("Starting Scene Load");
        FakeLoad = true;
        loadedScenes.Add(scene);
        yield return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        GetResourcesFromScene(scene);
        yield return SceneManager.UnloadSceneAsync(scene);
        yield return Resources.UnloadUnusedAssets();
        loadedScenes.Remove(scene);
        FakeLoad = false;
        Log.Info("Scene Unload complete.");
    }

    static void GetResourcesFromScene(string sceneName)
    {
        var inThisScene = pendingLoad
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
                pendingLoad.Remove(x);
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