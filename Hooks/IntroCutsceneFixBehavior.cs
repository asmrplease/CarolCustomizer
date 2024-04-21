using CarolCustomizer.Utils;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CarolCustomizer.Hooks;
internal class IntroCutsceneFixBehavior : IDisposable
{
    private GameObject clothingFolder;

    public IntroCutsceneFixBehavior(GameObject clothingFolder)
    {
        this.clothingFolder = clothingFolder;   

        SceneManager.sceneLoaded += OnSceneLoad;
        SceneManager.sceneUnloaded += OnSceneUnload;
    }

    private void OnSceneUnload(Scene arg0)
    {
        if (!clothingFolder) return;
        if (clothingFolder.activeSelf) return;
        clothingFolder.SetActive(true);
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode arg1)
    {
        if (scene.name != Constants.IntroCutsceneName) return;
        clothingFolder.SetActive(false);
    }

    public void Dispose()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
        SceneManager.sceneUnloaded -= OnSceneUnload;
    }
}
