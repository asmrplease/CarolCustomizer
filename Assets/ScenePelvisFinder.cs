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
public class ScenePelvisFinder : IDisposable
{
    HashSet<PelvisWatchdog> allWatchdogs = [];

    public ScenePelvisFinder(Transform parent)
    {
        SceneManager.sceneLoaded += FindAllPelvises;
    }

    void FindAllPelvises(Scene scene, LoadSceneMode mode)
    {
        Log.Debug("FindAllPelvises");
        if (mode == LoadSceneMode.Additive) return;
        if (scene.name == Constants.MenuSceneName) return;
        
        Resources
            .FindObjectsOfTypeAll<GameObject>()
            .Where(x => x.name == "CarolPelvis")
            .Select(PelvisWatchdog.GetAddWatchdog)
            .ForEach(x => allWatchdogs.Add(x));
    }

    public void Dispose()
    {
        Log.Debug("ScenePelvisFinder.Dispose()");
        SceneManager.sceneLoaded -= FindAllPelvises;
        allWatchdogs
            .Where(x => x)
            .ForEach(GameObject.Destroy);
    }
}
