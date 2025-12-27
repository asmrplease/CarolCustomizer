using System;
using UnityEngine;
using CarolCustomizer.Utils;
using UnityEngine.UIElements;

namespace CarolCustomizer.Assets;
public class UITKAssetLoader : IDisposable
{
    const string ListItemAddress = "assets/ui/listitem.uxml";
    const string TreeViewAddress = "Assets/UI/TreeView.uxml";
    const string ContextMenuAddress = "Assets/UI/ContextMenu.uxml";
    const string PanelSettingsAddress = "Assets/UI/New Panel Settings.asset";
    
    AssetBundle assetBundle;
    
    public VisualTreeAsset ListItem { get; private set; }
    public VisualTreeAsset TreeView { get; private set; }
    public VisualTreeAsset ContextMenu { get; private set; }
    public PanelSettings PanelSettings { get; private set; }    


    public UITKAssetLoader()
    {
        Log.Info("Loading UI assets.");
        try { Load(); }
        catch (Exception e)
        {
            Log.Error(e.Message);
            Dispose();
            return;
        }
        Log.Info("UITK loaded successfully.");
    }

    public void Dispose()
    {
        if (assetBundle) assetBundle.Unload(false);
        Log.Info("UI unloaded.");
    }

    void Load()
    {
        Log.Debug("UITKAssetLoader.Load()");
        assetBundle = AssetBundle.LoadFromFile(Constants.UITKAssetPath);
        if (!assetBundle) { Log.Error($"Failed to load UI Toolkit AssetBundle from {Constants.AssetFolderPath}"); return; }

        this.ListItem = assetBundle.LoadAsset<VisualTreeAsset>(ListItemAddress);
        this.TreeView = assetBundle.LoadAsset<VisualTreeAsset>(TreeViewAddress);
        this.ContextMenu = assetBundle.LoadAsset<VisualTreeAsset>(ContextMenuAddress);  
        this.PanelSettings = assetBundle.LoadAsset<PanelSettings>(PanelSettingsAddress);
        Log.Debug("UITKAssetLoader.Load() Complete.");
    }
}
