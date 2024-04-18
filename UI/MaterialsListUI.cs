using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CarolCustomizer.Behaviors;
using CarolCustomizer.Assets;
using CarolCustomizer.Utils;
using CarolCustomizer.Behaviors.Settings;

namespace CarolCustomizer.UI;
internal class MaterialsListUI : MonoBehaviour
{
    private static readonly string listRootAddress = "Scroll View/Viewport/Content";

    MaterialManager materialManager;
    TabbedUIAssetLoader loader;
    DynamicContextMenu contextMenu;

    Transform listRoot;

    List<ReadOnlyMatUI> materialUIs = new();

    public void Constructor(TabbedUIAssetLoader loader, MaterialManager materialManager, DynamicContextMenu contextMenu)
    {
        this.materialManager = materialManager;
        this.loader = loader;
        this.contextMenu = contextMenu;

        listRoot = this.transform.Find(listRootAddress);
    }

    private void OnEnable()
    {
        if (!listRoot) { Log.Warning("MaterialsListUI was null during OnEnable"); return; }
        //get the world materials from the material manager
        foreach (var item in materialUIs) { if (item) GameObject.Destroy(item.gameObject); }
        materialUIs.Clear();

        var materials = materialManager.ListMaterials();
        if (materials is null) return;

        foreach (var material in materialManager.ListMaterials())
        {
            var listElementGO = GameObject.Instantiate(loader.AccessoryListElement, listRoot);
            var matUI = listElementGO.AddComponent<ReadOnlyMatUI>();
            matUI.Constructor(material, materialManager, contextMenu);
            materialUIs.Add(matUI);
        }
    }
}
