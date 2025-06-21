using System;
using UnityEngine;
using CarolCustomizer.Utils;

namespace CarolCustomizer.Assets;
public class UIAssetLoader : IDisposable
{
    const string uiContainerAddress = "Assets/Mods/TabUI/Accessory Canvas.prefab";
    const string accessoryButtonAddress = "Assets/Mods/TabUI/Blank Accessory.prefab";
    const string outfitButtonAddress = "Assets/Mods/TabUI/Blank Outfit.prefab";
    const string contextMenuAddress = "Assets/Mods/TabUI/Context Menu.prefab";
    const string outfitViewAddress = "Assets/Mods/TabUI/OutfitView 1.prefab";
    const string materialsViewAddress = "Assets/Mods/TabUI/MaterialsView.prefab";
    const string recipesViewAddress = "Assets/Mods/TabUI/RecipesView.prefab";
    const string settingsViewAddress = "Assets/Mods/TabUI/SettingsView.prefab";
    const string filenameDialogueAddress = "Assets/Mods/TabUI/FilenameDialogue.prefab";
    const string messageDialogueAddress = "Assets/Mods/TabUI/MessageDialogue.prefab";
    const string pirateIconAddress = "assets/mods/tabui/pirateicon.png";

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
    public GameObject HairstyleView { get; private set; }
    public GameObject FilenameDialogue { get; private set; }
    public GameObject MessageDialogue { get; private set; }
    public Sprite PirateIcon { get; private set; }

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
        ContextMenu = assetBundle.LoadAsset<GameObject>(contextMenuAddress);
        OutfitView = assetBundle.LoadAsset<GameObject>(outfitViewAddress);
        MaterialsView = assetBundle.LoadAsset<GameObject>(materialsViewAddress);
        RecipesView = assetBundle.LoadAsset<GameObject>(recipesViewAddress);
        SettingsView = assetBundle.LoadAsset<GameObject>(settingsViewAddress);
        FilenameDialogue = assetBundle.LoadAsset<GameObject>(filenameDialogueAddress);
        MessageDialogue = assetBundle.LoadAsset<GameObject>(messageDialogueAddress);
        PirateIcon = assetBundle.LoadAsset<Sprite>(pirateIconAddress);
        ContextMenuButton = ContextMenu.transform.GetChild(0).gameObject;
    }
}
