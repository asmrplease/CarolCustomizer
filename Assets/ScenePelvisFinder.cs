using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CarolCustomizer.Assets;

public class ScenePelvisFinder : IDisposable
{
    public ScenePelvisFinder(Transform parent)
    {
        SceneManager.sceneLoaded += FindAllPelvises;
    }

    void FindAllPelvises(Scene scene, LoadSceneMode mode)
    {
        Log.Debug("FindAllPelvises");
        if (mode == LoadSceneMode.Additive) return;

        Resources
            .FindObjectsOfTypeAll<GameObject>()
            .Where(x => x.name == Constants.Pelvis)
            //.Select(PelvisWatchdog.GetAddWatchdog)
            .ForEach(x => PelvisWatchdog.GetAddWatchdog(x));
    }

    public void Dispose()
    {
        Log.Debug("ScenePelvisFinder.Dispose()");
        SceneManager.sceneLoaded -= FindAllPelvises;
    }
}
