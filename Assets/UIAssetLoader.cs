using System;
using UnityEngine;
using CarolCustomizer.Utils;

namespace CarolCustomizer.Assets;
public class UIAssetLoader : IDisposable
{
    const string uiContainerAddress = "Assets/Mods/TabUI/Accessory Canvas.prefab";
    const string accessoryButtonAddress = "Assets/Mods/TabUI/Blank Accessory.prefab";
    const string outfitButtonAddress = "Assets/Mods/TabUI/Blank Outfit.prefab";
    const string listItemAddress = "Assets/Mods/TabUI/ListItem.prefab";
    const string listItemUIAddress = "Assets/Mods/TabUI/ListItemUI.prefab";
    const string contextMenuAddress = "Assets/Mods/TabUI/Context Menu.prefab";
    const string outfitViewAddress = "Assets/Mods/TabUI/OutfitView.prefab";
    const string materialsViewAddress = "Assets/Mods/TabUI/MaterialsView.prefab";
    const string recipesViewAddress = "Assets/Mods/TabUI/RecipesView.prefab";
    const string settingsViewAddress = "Assets/Mods/TabUI/SettingsView.prefab";
    const string filenameDialogueAddress = "Assets/Mods/TabUI/FilenameDialogue 1.prefab";
    const string messageDialogueAddress = "Assets/Mods/TabUI/MessageDialogue.prefab";
    const string eyedropperCursorAddress = "Assets/Mods/TabUI/eyedropper32.png";
    const string customizeNPCAddress = "Assets/Mods/TabUI/Customize NPC.prefab";

    AssetBundle assetBundle;

    public GameObject UIContainer { get; private set; }
    public GameObject AccessoryListElement { get; private set; }
    public GameObject OutfitListElement { get; private set; }
    public GameObject HairstyleListElement { get; private set; }
    public GameObject ContextMenu { get; private set; }
    public GameObject ContextMenuButton { get; private set; }
    public GameObject OutfitView { get; private set; }
    public GameObject MaterialsView { get; private set; }
    public GameObject RecipesView { get; private set; }
    public GameObject SettingsView { get; private set; }
    public GameObject CustomizeNPC { get; private set; }
    public GameObject HairstyleView { get; private set; }
    public GameObject FilenameDialogue { get; private set; }
    public GameObject MessageDialogue { get; private set; }
    public Texture2D CursorTexture { get; private set; }
    public GameObject ListItem { get; private set; }
    public GameObject ListItemUI { get; private set; }


    public UIAssetLoader()
    {
        Log.Info("Loading UI assets.");
        try { Load(); }
        catch (Exception e)
        {
            Log.Error(e.Message);
            Dispose();
            return;
        }
        Log.Info("UI loaded successfully.");
    }

    public void Dispose()
    {
        if (assetBundle) assetBundle.Unload(false);
        Log.Info("UI unloaded.");
    }

    void Load()
    {
        assetBundle = AssetBundle.LoadFromFile(Constants.UIAssetPath);
        if (!assetBundle) { Log.Error($"Failed to load TabbedUI AssetBundle from {Constants.AssetFolderPath}"); return; }

        UIContainer = assetBundle.LoadAsset<GameObject>(uiContainerAddress);
        AccessoryListElement = assetBundle.LoadAsset<GameObject>(accessoryButtonAddress);
        OutfitListElement = assetBundle.LoadAsset<GameObject>(outfitButtonAddress);
        ListItem = assetBundle.LoadAsset<GameObject>(listItemAddress);
        ListItemUI = assetBundle.LoadAsset<GameObject>(listItemUIAddress);
        ContextMenu = assetBundle.LoadAsset<GameObject>(contextMenuAddress);
        OutfitView = assetBundle.LoadAsset<GameObject>(outfitViewAddress);
        MaterialsView = assetBundle.LoadAsset<GameObject>(materialsViewAddress);
        RecipesView = assetBundle.LoadAsset<GameObject>(recipesViewAddress);
        SettingsView = assetBundle.LoadAsset<GameObject>(settingsViewAddress);
        FilenameDialogue = assetBundle.LoadAsset<GameObject>(filenameDialogueAddress);
        MessageDialogue = assetBundle.LoadAsset<GameObject>(messageDialogueAddress);
        CursorTexture = assetBundle.LoadAsset<Texture2D>(eyedropperCursorAddress);
        CustomizeNPC = assetBundle.LoadAsset<GameObject>(customizeNPCAddress);
        ContextMenuButton = ContextMenu.transform.GetChild(0).gameObject;

        ListItem.SetActive(false);
    }
}
