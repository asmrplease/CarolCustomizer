using CarolCustomizer.Assets;
using CarolCustomizer.Contracts;
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

    public static void Show(IContextMenuActions target)
    {
        Log.Debug("UITKContextMenu.Show()");
        //set menu position relative to cursor

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
        //get asset loader and assets
        Instance.assetLoader = assetLoader;
        var uidoc = Instance.gameObject.AddComponent<UIDocument>();
        uidoc.visualTreeAsset = assetLoader.ContextMenu;
        uidoc.sourceAsset = assetLoader.ContextMenu;
        uidoc.panelSettings = assetLoader.PanelSettings;//TODO: do we need inidividual panel settings per ui element?
        var root = uidoc.rootVisualElement;
        Instance.listView = root.Q<ListView>();
        Instance.listView.fixedItemHeight = 100;

        //set up button handler
        Instance.listView.makeItem = () => new Button();
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
