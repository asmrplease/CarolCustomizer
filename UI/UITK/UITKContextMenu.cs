using CarolCustomizer.Assets;
using CarolCustomizer.Contracts;
using CarolCustomizer.UI.Legacy.Main;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CarolCustomizer.UI.UITK;
internal class UITKContextMenu : MonoBehaviour
{
    static UITKContextMenu Instance;
    UITKAssetLoader assetLoader;
    ListView listView;
    List<ContextButton> currentItems;

    void Awake() => MenuToggle.OnMenuToggle += HandleMenuToggle;
    void OnDestroy() => MenuToggle.OnMenuToggle -= HandleMenuToggle;

    private void HandleMenuToggle(bool state)
    {
        if (!state) Hide();
    }

    public static void Show(IContextMenuActions target)
    {
        Log.Debug("UITKContextMenu.Show()");
        //set menu position relative to cursor
        //Instance.listView.style.position = new StyleEnum<UnityEngine.UIElements.Position>(Position.Absolute);
        var mouse = Input.mousePosition;
        Instance.listView.style.translate = new Translate(mouse.x, Screen.height - mouse.y);

        //set root items of list 
        Instance.currentItems = target.GetContextMenuItems();
        Instance.listView.itemsSource = Instance.currentItems;

        //enable and refresh the ui elements
        Instance.listView.Rebuild();
        Instance.listView.visible = true;
    }

    public static void Hide()
    {
        //disable this gameobject?
        Instance.listView.visible = false;
    }

    public static UITKContextMenu Constructor(UITKAssetLoader assetLoader)
    {
        if (Instance) return Instance;

        var idk = new GameObject();
        Instance = idk.AddComponent<UITKContextMenu>();
        Instance.assetLoader = assetLoader;
        var uidoc = Instance.gameObject.AddComponent<UIDocument>();
        uidoc.visualTreeAsset = assetLoader.ContextMenu;
        uidoc.sourceAsset = assetLoader.ContextMenu;
        uidoc.panelSettings = assetLoader.PanelSettings;//TODO: do we need inidividual panel settings per ui element?
        var root = uidoc.rootVisualElement;
        Instance.listView = root.Q<ListView>();
        Instance.listView.fixedItemHeight = 100;

        //set up button handler
        Instance.listView.makeItem = () => 
        {
            var button = new Button();
            button.style.width = 350;
            return button;
        };
        Instance.listView.bindItem = (element, index) =>
        {
            var (text, action) = Instance.currentItems[index];
            var button = element.Q<Button>();
            button.text = text;
            button.clicked += () => ActionWrapper(action);
            button.clicked += Hide;
        };

        return Instance;
    }

    static void ActionWrapper(Action action)
    {
        try { action(); } catch (Exception e) { Log.Error(e.Message); }
    }

}
