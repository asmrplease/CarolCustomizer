using CarolCustomizer.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Main;
internal class LoadingIndicator : MonoBehaviour
{
    GameObject indicatorObject;
    readonly HashSet<Guid> loadingProcesses = [];
    RectTransform rect; 

    internal LoadingIndicator Constructor(Transform uiRoot)
    {
        StartCoroutine(WaitForUI(uiRoot));
        return this;
    }

    IEnumerator WaitForUI(Transform uiRoot)
    {
        yield return new WaitUntil(() => GameManager.manager);
        yield return new WaitUntil(() => GameManager.manager.loadingScreen);

        var source = GameManager.manager.loadingScreen.transform.Find("loadingthing");
        if (!source) { Log.Error("Failed to find loading indicator in GameManager"); yield break; }

        indicatorObject = Instantiate(source.gameObject, uiRoot.parent);
        var outline = indicatorObject.AddComponent<UnityEngine.UI.Outline>();
        var shadow = indicatorObject.GetComponent<Shadow>();
        this.rect = indicatorObject.GetComponent<RectTransform>();
        var image = indicatorObject.GetComponent<Image>();
        Destroy(shadow);
        outline.effectColor = Color.white;
        image.color = Constants.DefaultColor;
        rect.pivot = Vector2.zero;
        indicatorObject.transform.position = new Vector3(256, 0, 0);
        rect.localScale = Vector3.one;
        Log.Debug($"position is now {rect.anchoredPosition}");
        indicatorObject.SetActive(false);
        yield break;
    }


    internal Guid NotifyLoadingStart()
    {
        var guid = new Guid();
        loadingProcesses.Add(guid);
        indicatorObject?.SetActive(true);
        return guid;
    }

    internal void NotifyLoadingComplete(Guid guid)
    {
        loadingProcesses.Remove(guid);
        if (loadingProcesses.Any()) return;

        indicatorObject?.SetActive(false);
    }

}
