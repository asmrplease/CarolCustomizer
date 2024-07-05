using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CarolCustomizer.Assets;
/// <summary>
/// Class responsible for collecting references to all optional assets.
/// Primarily outfits.
/// </summary>
public class NPCInstanceCreator : IDisposable
{
    public static List<string> actressSearchRoots = new() { "CUTSCENES", "SECTOR_STORMYSEA" };

    public static List<PelvisWatchdog> dedicatedActresses = new();
    private static List<PelvisWatchdog> carolBotPrefabs = new();
    

    #region Lifecycle
    public NPCInstanceCreator(Transform parent)
    {
        SceneManager.sceneLoaded += FindDedicatedActresses;
        SceneManager.sceneLoaded += FindBotEntities;
    }

    public void Dispose()
    {
        SceneManager.sceneLoaded -= FindDedicatedActresses;
        SceneManager.sceneLoaded -= FindBotEntities;

        foreach (var actress in dedicatedActresses) { if (actress) GameObject.Destroy(actress); }
        foreach (var botPrefab in carolBotPrefabs) { if (botPrefab) GameObject.Destroy(botPrefab); }
    }
    #endregion

    //TODO: are gamemanager.playeractor and playeractorreplace useful here?
    public static void FindDedicatedActresses(Scene scene, LoadSceneMode arg1)
    {
        Log.Debug("FindDedicatedActressess");
        var count = dedicatedActresses.RemoveAll(x => !x);
        Log.Debug($"Removed {count} dead actressess");
        if (scene.name == Constants.MenuSceneName) return;

        Log.Debug("Searching for dedicated actresses...");
        var list = Resources.FindObjectsOfTypeAll<Slate.Character>();
        Log.Debug($"{list.Length} Slate.Character components in Resources");
        var actresses = list.Where(x => x.name.Contains("CAROL")).Select(x=>x.transform).ToList();
        Log.Debug($"{actresses.Count()} slate actresseses found.");

        foreach (string rootAddress in actressSearchRoots)
        {
            actresses.AddRange(FindPelvisesInRootObject(scene, rootAddress));
        }
        Log.Debug($"{actresses.Count()} total actresseses found.");


        foreach (var actress in actresses) 
        {
            Log.Debug($"building dedicated actress for: {actress.name}");
            Transform target = actress;
            if (target.name != "CarolPelvis") target = target.RecursiveFindTransform(x => x.name == "CarolPelvis");
            if (!target) { Log.Warning($"failed to find pelvis for {actress.name}"); return; }
            var watchdog = target.gameObject.AddComponent<PelvisWatchdog>();
            dedicatedActresses.Add(watchdog);
        }
    }

    static IEnumerable<Transform> FindPelvisesInRootObject(Scene scene, string rootObjectName)
    {
        var componentless = new List<Transform>();
        var roots = scene.GetRootGameObjects();

        if (!roots.Any(x => x.name == rootObjectName)) { return componentless; }

        Log.Debug($"looking for carols in {rootObjectName}");
        var cutsceneGO = scene.GetRootGameObjects().First(x => x.name == rootObjectName);
            
        cutsceneGO.transform.RecursiveFindTransforms(x => x.name == "CarolPelvis", ref componentless);
        Log.Debug($"found {componentless.Count()} componentless pelvises");
        var parents = componentless.Select(x => x.parent.parent);
        return parents;
    }

    public static void FindBotEntities(Scene arg0, LoadSceneMode arg1)
    {
        var entities = Resources.FindObjectsOfTypeAll<Entity>().
            Where(entity => 
                    entity.name.StartsWith("Carol_Robot")
                && !entity.name.Contains("(Clone)")
                && !carolBotPrefabs.Any(x=>x.name == entity.name));

        foreach (var entity in entities) { OnCarolBotDetected(entity.gameObject); }
    }

    static void OnCarolBotDetected(GameObject botPrefab)
    {
        //remove voice componenets which are spamming console
        //do we still need to do this?
        var voices = botPrefab.GetComponents<Voice>();
        foreach (var voice in voices) { voice.enabled = false; }

        botPrefab.hideFlags = HideFlags.HideAndDontSave;

        var watchdog = botPrefab
            .transform
            .RecursiveFindTransform
            (x => x.name == "CarolPelvis")
            .gameObject
            .AddComponent<PelvisWatchdog>();

        carolBotPrefabs.Add(watchdog);
    }
}
