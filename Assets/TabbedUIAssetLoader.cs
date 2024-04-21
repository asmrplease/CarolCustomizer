using System;
using UnityEngine;
using CarolCustomizer.Utils;
using System.Globalization;
using System.IO;

namespace CarolCustomizer.Assets;
public class TabbedUIAssetLoader : IDisposable
{
    private static readonly string assetBundleName = "tabui.ui";
    private static readonly string folderName = "Mods";

    private static readonly string uiContainerAddress = "Assets/Mods/TabUI/Accessory Canvas.prefab";
    private static readonly string accessoryButtonAddress = "Assets/Mods/TabUI/Blank Accessory.prefab";
    private static readonly string outfitButtonAddress = "Assets/Mods/TabUI/Blank Outfit.prefab";
    private static readonly string contextMenuAddress = "Assets/Mods/TabUI/Context Menu.prefab";
    private static readonly string outfitViewAddress = "Assets/Mods/TabUI/OutfitView.prefab";
    private static readonly string materialsViewAddress = "Assets/Mods/TabUI/MaterialsView.prefab";
    private static readonly string recipesViewAddress = "Assets/Mods/TabUI/RecipesView.prefab";
    private static readonly string settingsViewAddress = "Assets/Mods/TabUI/SettingsView.prefab";
    private static readonly string filenameDialogueAddress = "Assets/Mods/TabUI/FilenameDialogue.prefab";
    private static readonly string messageDialogueAddress = "Assets/Mods/TabUI/MessageDialogue.prefab";

    public GameObject UIContainer { get; private set; }
    public GameObject AccessoryListElement { get; private set; }
    public GameObject OutfitListElement { get; private set; }
    public GameObject ContextMenu { get; private set; }
    public GameObject ContextMenuButton { get; private set; }
    public GameObject OutfitView { get; private set; }
    public GameObject MaterialsView { get; private set; }
    public GameObject RecipesView { get; private set; }
    public GameObject SettingsView { get; private set; }
    public GameObject FilenameDialogue { get; private set; }
    public GameObject MessageDialogue { get; private set; }

    AssetBundle assetBundle;

    public TabbedUIAssetLoader()
    {
        Log.Debug("loading tabbed UI assets");

        try { Load(); }
        catch (Exception e)
        {
            Log.Error(e.Message);
            Dispose();
            return;
        }

        Log.Debug("done");
    }

    public void Dispose()
    {
        if (assetBundle) assetBundle.Unload(false);
    }

    private void Load()
    {
        assetBundle = LoadAssetBundle();
        if (!assetBundle) { Log.Error("Failed to load TabbedUI AssetBundle."); return; }

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

        ContextMenuButton = ContextMenu.transform.GetChild(0).gameObject;
    }

    private AssetBundle LoadAssetBundle()
    {
        string applicationPath = Directory.GetParent(Application.dataPath).FullName;
        string relativePath = Path.Combine(folderName, assetBundleName);

        string path = Path.Combine(applicationPath, relativePath).ToLower(CultureInfo.InvariantCulture);

        return AssetBundle.LoadFromFile(path);
    }

}
