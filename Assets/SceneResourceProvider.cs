using CarolCustomizer.Models.Materials;
using CarolCustomizer.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CarolCustomizer.Assets;
internal class SceneResourceProvider
{
    //we want a list of materials to load, grouped by the scene that they're sourced from
    readonly static List<MaterialDescriptor> pendingLoad = [];
    readonly static List<MaterialDescriptor> loaded = [];

    static void GetResourcesFromScene(string sceneName)
    {
        var inThisScene = pendingLoad
            .Where(x => x.Source == sceneName)
            .Select(x => x.Name)
            .ToList();
        Log.Debug("Assets pending load:");
        inThisScene.ForEach(Log.Debug);
        Resources.FindObjectsOfTypeAll<Material>()
            .Where(x => inThisScene.Contains(x.name))
            .Select(x => new MaterialDescriptor(x, sceneName, MaterialDescriptor.SourceType.World))
            .ForEach(x =>
            {
                x.referenceMaterial.hideFlags = HideFlags.HideAndDontSave;
                loaded.Add(x);
                pendingLoad.Remove(x);
            });
    }

    internal static void VolcanoTest()
    {
        CCPlugin.CoroutineRunner.StartCoroutine(TestLoad2());
    }

    static IEnumerator TestLoad2()
    {
        string scene = "volcano";
        string matName = "Lava_3_Vertex Color Only 11";
        Log.Info("Target materials currently loaded:");
        Resources.FindObjectsOfTypeAll<Material>()
            .Where(x => x.name.Contains(matName))
            .ForEach(x => Log.Info(x.name));
        pendingLoad.Add(new MaterialDescriptor(matName, scene, MaterialDescriptor.SourceType.World));

        Log.Info("Starting Scene Load");
        var request = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        //request.allowSceneActivation = false;
        yield return new WaitUntil(() => request.progress > 0.89f);

        GetResourcesFromScene(scene);
        Log.Info("Target materials currently loaded:");
        loaded
            .Where(x => x.Name.Contains(matName))
            .Select(x => x.Name)
            .ForEach(Log.Info);
        yield return SceneManager.UnloadSceneAsync(scene);
        Log.Info("Scene Unload complete.");
    }

    static IEnumerator TestLoad()
    {
        //load volcano
        Log.Info("Target materials currently loaded:");
        Resources.FindObjectsOfTypeAll<Material>()
            .Where(x => x.name.Contains("Lava_3_Vertex Color Only 11"))
            .ForEach(x => Log.Info(x.name));
        Log.Info("Starting Scene Load");
        var request = SceneManager.LoadSceneAsync("volcano", LoadSceneMode.Additive);
        request.allowSceneActivation = false;
        yield return request;
        Log.Info("Target materials currently loaded:");
        Resources.FindObjectsOfTypeAll<Material>()
            .Where(x => x.name.Contains("Lava_3_Vertex Color Only 11"))
            .ForEach(x => Log.Info(x.name));
        Log.Info("End target materials. Starting scene unload.");
        yield return SceneManager.UnloadSceneAsync("volcano");
        Log.Info("Scene unloaded");
        
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
