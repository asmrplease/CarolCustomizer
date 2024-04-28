using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors;
using CarolCustomizer.UI.Main;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace CarolCustomizer.UI.Materials;
internal class MaterialsListUI : MonoBehaviour
{
    private static readonly string listRootAddress = "Scroll View/Viewport/Content";

    MaterialManager materialManager;
    UIAssetLoader loader;
    DynamicContextMenu contextMenu;

    Transform listRoot;

    List<ReadOnlyMatUI> materialUIs = new();

    public void Constructor(UIAssetLoader loader, MaterialManager materialManager, DynamicContextMenu contextMenu)
    {
        this.materialManager = materialManager;
        this.loader = loader;
        this.contextMenu = contextMenu;

        listRoot = transform.Find(listRootAddress);
    }

    private void OnEnable()
    {
        if (!listRoot) { Log.Warning("MaterialsListUI was null during OnEnable"); return; }
        //get the world materials from the material manager
        foreach (var item in materialUIs) { if (item) Destroy(item.gameObject); }
        materialUIs.Clear();

        var materials = materialManager.ListMaterials();
        if (materials is null) return;

        foreach (var material in materialManager.ListMaterials())
        {
            var listElementGO = Instantiate(loader.AccessoryListElement, listRoot);
            var matUI = listElementGO.AddComponent<ReadOnlyMatUI>();
            matUI.Constructor(material, materialManager, contextMenu);
            materialUIs.Add(matUI);
        }
    }
}
