using CarolCustomizer.Contracts;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Main;
public class ContextMenu : MonoBehaviour
{
    #region Dependencies
    GameObject buttonPrefab;
    #endregion

    #region Local Components
    List<Button> buttonList = new();
    Transform buttonContainer;
    #endregion

    #region Lifecycle
    private void Awake()
    {
        foreach (Transform child in transform) { Destroy(child.gameObject); }
        gameObject.SetActive(false);
    }

    public ContextMenu Constructor(GameObject buttonPrefab)
    {
        this.buttonPrefab = buttonPrefab;
        buttonContainer = transform;
        this.gameObject.SetActive(false);
        return this;
    }
    #endregion

    #region Event Handling
    public void Show(IContextMenuActions menuItems)
    {
        if (menuItems is null) return;
        if (gameObject.activeSelf) { Hide(); }

        foreach ((string name, UnityAction action) in menuItems.GetContextMenuItems())
        {
            GameObject buttonGO = Instantiate(buttonPrefab, buttonContainer);
            if (!buttonGO) { Log.Error("failed to instantiate button prefab."); return; }
            Button buttonComponent = buttonGO.GetComponent<Button>();
            if (!buttonComponent) { Log.Error("failed to find button component."); return; }

            buttonComponent.GetComponentInChildren<Text>(true).text = name;
            buttonComponent.onClick.AddListener(action);
            buttonComponent.onClick.AddListener(Hide);
            buttonList.Add(buttonComponent);
        }
        gameObject.SetActive(true);
        transform.position = Input.mousePosition;
        Canvas.ForceUpdateCanvases();

        var rect = transform as RectTransform;
        var yOffscreen = rect.anchoredPosition.y - rect.sizeDelta.y;
        Log.Debug($"{rect.anchoredPosition.y} - {rect.sizeDelta.y} = {yOffscreen}");
        if (yOffscreen >= 0) return;
        Log.Debug("Context Menu is at least partially offscreen, adjusting");
        var position = rect.anchoredPosition;
        position.y = rect.sizeDelta.y;
        rect.anchoredPosition = position;
        Log.Debug($"new y position = {position.y}");
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        foreach (var button in buttonList) { Destroy(button.gameObject); }
        buttonList.Clear();
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0)
            || Input.GetMouseButtonUp(1)
            || Input.GetMouseButtonUp(2)
            || Input.GetKeyDown(KeyCode.Escape))
        { Hide(); }
    }
    #endregion
}
