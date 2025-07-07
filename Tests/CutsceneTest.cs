using CarolCustomizer.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CarolCustomizer.Tests;
internal class CutsceneTest
{
    //For each campaign scene,
        //For each cutscene in scene
            //start cutscene
            //wait 10s after end of cutscene
    static void LoadScene(string sceneName) => GameManager.manager.LoadScene(sceneName, TestData());
    static SaveManager.SaveData TestData() => 
        //SaveManager.manager.data[SaveManager.manager.saveSlotCurrent];
        SaveManager.manager.NewSave();
    static int sceneIndex = 0;
    static bool readyForNextScene = true;
    static bool readyForNextCutscene = true;

    static List<string> GetCampaignSceneNames()
    {
        return GameManager.manager
            .scenes
            .Where(x => x.isStory)
            .Select(x => x.sceneName)
            .ToList();
    }

    internal static void RunAllCutscenes()
    {
        CCPlugin.CoroutineRunner.StartCoroutine(LoadAllScenes());
    }

    //call load cutscene
    //wait for scene to load
    //get cutscenes
    //foreach cutscene
        //start cutscene
        //wait for cutscene end
        //wait 10s 

    static IEnumerator LoadAllScenes()
    {
        foreach (var scene in GetCampaignSceneNames())
        {
            SceneManager.sceneLoaded += HandleSceneChanged;
            readyForNextScene = false;
            LoadScene(scene);
            yield return new WaitUntil(() => readyForNextScene);
        }
    }

    static IEnumerator PlayAllCutscenes(Slate.Cutscene[] cutscenes) 
    {
        foreach (var scene in cutscenes)
        {
            readyForNextCutscene = false;
            scene.Play(() => readyForNextCutscene = true);
            yield return new WaitUntil(() => readyForNextCutscene);
            yield return new WaitForSeconds(5);
        }
        readyForNextScene = true;
    }

    private static void HandleSceneChanged(Scene scene, LoadSceneMode arg1)
    {
        SceneManager.sceneLoaded -= HandleSceneChanged;
        Log.Info($"Cutscenes in {scene.name}:");
        var cutscenes = Resources.FindObjectsOfTypeAll<Slate.Cutscene>();
        cutscenes
            .Select(x => x.gameObject.name)
            .ForEach(Log.Info);
        Resources.FindObjectsOfTypeAll<Slate.PlayCutsceneOnTrigger>()
            .ForEach(x=> x.enabled = false);
        CCPlugin.CoroutineRunner.StartCoroutine(PlayAllCutscenes(cutscenes));
    }
}
