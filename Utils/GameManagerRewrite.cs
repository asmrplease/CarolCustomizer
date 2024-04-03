using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CarolCustomizer.Utils;
public class GameManagerRewrite : MonoBehaviour
{
    List<Coroutine> audioCoroutines = new();

    public void LoadAssetsFunction()
    {
        Log.Warning("Running LoadAssets");
        StartCoroutine(LoadAssets());
    }

    public IEnumerator LoadAssets()
    {
        float time = Time.time;
        float time2 = Time.time;
        GameManager.manager.carolModel = GameManager.manager.GetOutfit("carol_pyjamabasic");
        string[] weaponsFiles = this.RemoveDuplicates(Resources.Load<TextAsset>("weapons").text.Split(';', StringSplitOptions.None));
        yield return null;
        string[] gadgetsFiles = this.RemoveDuplicates(Resources.Load<TextAsset>("gadgets").text.Split(';', StringSplitOptions.None));
        yield return null;
        string[] characterFiles = this.RemoveDuplicates(Resources.Load<TextAsset>("multiplayercharacters").text.Split(';', StringSplitOptions.None));
        yield return null;
        float weaponTime = Time.time;
        GameManager.manager.weapons = new List<GameManager.Asset>();
        foreach (string name in weaponsFiles)
        {
            float time3 = Time.time;
            GameManager.manager.weapons.Add(new GameManager.Asset
            {
                name = name
            });
        }
        GameManager.manager.weapons = (from w in GameManager.manager.weapons
                        where w != null
                        select w).ToList<GameManager.Asset>();
        float weaponTimeEnd = Time.time;
        float gadgetTime = Time.time;
        foreach (string name2 in gadgetsFiles)
        {
            float time4 = Time.time;
            GameManager.manager.gadgets.Add(new GameManager.Asset
            {
                name = name2
            });
        }
        GameManager.manager.gadgets = (from w in GameManager.manager.gadgets
                        where w != null
                        select w).ToList<GameManager.Asset>();
        float gadgetTimeEnd = Time.time;
        float outfitTime = Time.time;
        float outfitTimeEnd = Time.time;
        float time5 = Time.time;
        foreach (string name3 in characterFiles)
        {
            float time6 = Time.time;
            GameManager.manager.characterSelection.Add(new GameManager.Asset
            {
                name = name3
            });
        }
        float time7 = Time.time;
        GameManager.manager.outfits = (from w in GameManager.manager.outfits
                        where w != null
                        select w).ToList<GameManager.Asset>();
        yield return null;
        string[] dialogues = this.RemoveDuplicates(Resources.Load<TextAsset>("dialogues").text.Split(';', StringSplitOptions.None));
        yield return null;
        float voiceTime = Time.time;
        foreach (string str in dialogues)
        {
            audioCoroutines.Add(StartCoroutine(LoadAudio(str)));
        }
 
        float voiceTimeEnd = Time.time;
        float diaryTime = Time.time;
        yield return null;
        var menuRequest = Resources.LoadAsync<GameObject>("Carol/MenuPauseNew");
        yield return menuRequest;
        GameManager.manager.diary = menuRequest.asset as GameObject;
        yield return null;
        float diaryTimeEnd = Time.time;
        float carolTime = Time.time;
        yield return null;
        GameManager.manager.carolPrefab = Resources.Load<GameObject>("Carol/CAROL");
        yield return null;
        float carolTimeEnd = Time.time;
        float cameraTime = Time.time;
        yield return null;
        GameManager.manager.cameraPrefab = Resources.Load<GameObject>("Carol/Main camera");
        yield return null;
        float cameraTimeEnd = Time.time;
        float time9 = Time.time;
        yield return null;
        GameManager.manager.versusMatchUp = Resources.Load<GameObject>("VERSUSPANEL");
        yield return null;
        float time10 = Time.time;
        float loadingTime = Time.time;
        yield return null;
        GameManager.manager.loadingScreen = Resources.Load<GameObject>("LoadingScreen");
        yield return null;
        float loadingTimeEnd = Time.time;
        float unlockTime = Time.time;
        yield return null;
        GameManager.manager.unlockScreen = Resources.Load<GameObject>("UNLOCKpopup");
        yield return null;
        float time11 = Time.time;
        
        
        if (!Dialogue.hasController)
        {
            GameManager.manager.dialogueController = Util.Spawn(GameManager.manager.dialogueControllerPrefab);
            Dialogue.controller = GameManager.manager.dialogueController.GetComponent<Dialogue>();
        }
        Dialogue.controller.voiceLinesEn = GameManager.manager.dialogueLines;
        Dialogue.controller.voiceLinesFr = GameManager.manager.dialogueLines;
        Dialogue.controller.Init();
        //foreach (var routine in audioCoroutines) { yield return routine; }

        Log.Debug("Total time for weapons: " + (weaponTimeEnd - weaponTime).ToString());
        Log.Debug("Total time for gadgets: " + (gadgetTimeEnd - gadgetTime).ToString());
        Log.Debug("Total time for outfits: " + (outfitTimeEnd - outfitTime).ToString());
        Log.Debug("Total time for voiceclips: " + (voiceTimeEnd - voiceTime).ToString());
        Log.Debug("Total time for diary: " + (diaryTimeEnd - diaryTime).ToString());
        Log.Debug("Total time for Carol: " + (carolTimeEnd - carolTime).ToString());
        Log.Debug("Total time for camera: " + (cameraTimeEnd - cameraTime).ToString());
        Log.Debug("Total time for versus panel: " + (cameraTimeEnd - cameraTime).ToString());
        Log.Debug("Total time for loading screen: " + (loadingTimeEnd - loadingTime).ToString());
        Log.Debug("Total time for unlock screen: " + (time11 - unlockTime).ToString());
        GameManager.manager.assetsLoaded = true;
        if (SceneManager.GetActiveScene().name == "Loading_Startup")
        {
            GameManager.manager.LoadScene("Main_menu_new", null);
        }
        yield break;
    }

    private IEnumerator LoadAudio(string str)
    {
        float audioLoadTime = Time.time;
        var audioRequest = Resources.LoadAsync<AudioClip>("AUDIO/VF_DIALOGUES/VF/" + str);
        yield return audioRequest;
        GameManager.manager.dialogueLines.Add(audioRequest.asset as AudioClip);
        yield return null;
    }

    public string[] RemoveDuplicates(string[] strings)
    {
        List<string> list = new List<string>();
        foreach (string text in strings)
        {
            if (!list.Contains(text) && text != "")
            {
                list.Add(text);
            }
        }
        return list.ToArray();
    }
}
