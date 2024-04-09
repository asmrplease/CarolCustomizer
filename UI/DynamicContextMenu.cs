using CarolCustomizer.Contracts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CarolCustomizer.Utils;

namespace CarolCustomizer.UI;
public class DynamicContextMenu : MonoBehaviour
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
        foreach (Transform child in this.transform) { GameObject.Destroy(child.gameObject); }
        this.gameObject.SetActive(false);
    }

    public void Constructor(GameObject buttonPrefab)
    {
        this.buttonPrefab = buttonPrefab;
        this.buttonContainer = this.transform;
    }
    #endregion

    #region Event Handling
    public void Show(IContextMenuActions menuItems)
    {
        if (menuItems is null) return;
        if (this.gameObject.activeSelf) { Hide(); }

        foreach (var item in menuItems.GetContextMenuItems())
        {
            GameObject buttonGO = GameObject.Instantiate(buttonPrefab, buttonContainer);
            if (!buttonGO) { Log.Error("failed to instantiate button prefab."); return; }
            Button buttonComponent = buttonGO.GetComponent<Button>();
            if (!buttonComponent) { Log.Error("failed to find button component."); return; }

            buttonComponent.GetComponentInChildren<Text>(true).text = item.Key;
            buttonComponent.onClick.AddListener(item.Value);
            buttonComponent.onClick.AddListener(Hide);
            buttonList.Add(buttonComponent);
        }

        this.transform.position = Input.mousePosition;
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
        foreach (var button in buttonList) { GameObject.Destroy(button.gameObject); }
        buttonList.Clear();
    }

    private void Update()
    {
        if    (Input.GetMouseButtonUp(0) 
            || Input.GetMouseButtonUp(1)
            || Input.GetMouseButtonUp(2)
            || Input.GetKeyDown(KeyCode.Escape))
        { Hide(); }
    }
    #endregion
}
