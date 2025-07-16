using CarolCustomizer.Utils;
using System;
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
        var source = GameManager.manager.loadingScreen.transform.Find("loadingthing");
        if (!source) { Log.Error("Failed to find loading indicator in GameManager"); return this; }

        indicatorObject = Instantiate(source.gameObject, uiRoot.parent);
        var shadow = indicatorObject.GetComponent<Shadow>();
        Destroy(shadow);
        var outline = indicatorObject.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = Color.white;
        //outline.effect
        //outline.OutlineColor = Color.white;
        //outline
        this.rect = indicatorObject.GetComponent<RectTransform>();
        var image = indicatorObject.GetComponent<Image>();

        image.color = Constants.DefaultColor;
        //var pos = new Vector2(Screen.width / 2, Screen.height / 2);
        //Log.Debug($"Setting loading indicator position to {pos}");

        //rect.anchoredPosition = pos;
        rect.pivot = Vector2.zero;
        indicatorObject.transform.position = new Vector3(256, 0, 0);
        //indicatorObject.transform.position = new Vector3
        rect.localScale = Vector3.one;
        Log.Debug($"position is now {rect.anchoredPosition}");
        indicatorObject.SetActive(false);

        return this;
    }


    internal Guid NotifyLoadingStart()
    {
        var guid = new Guid();
        loadingProcesses.Add(guid);
        indicatorObject.SetActive(true);
        return guid;
    }

    internal void NotifyLoadingComplete(Guid guid)
    {
        loadingProcesses.Remove(guid);
        if (loadingProcesses.Any()) return;

        indicatorObject.SetActive(false);
    }

}
