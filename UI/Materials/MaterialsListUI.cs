using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.UI.Materials;
internal class MaterialsListUI : MonoBehaviour
{
    private static readonly string listRootAddress = "Scroll View/Viewport/Content";

    MaterialManager materialManager;
    UIAssetLoader loader;
    Eyedropper eyedropper;
    Main.ContextMenu contextMenu;

    Transform listRoot;

    List<ReadOnlyMatUI> materialUIs = new();

    public MaterialsListUI Constructor(UIAssetLoader loader, MaterialManager materialManager, Main.ContextMenu contextMenu)
    {
        this.materialManager = materialManager;
        this.loader = loader;
        this.contextMenu = contextMenu;
        this.eyedropper = this.gameObject.AddComponent<Eyedropper>();
        this.eyedropper.OnMaterialsFound += OnMaterialsChanged;
        listRoot = transform.Find(listRootAddress);
        return this;
    }

    private void OnMaterialsChanged(List<MaterialDescriptor> materials)
    {
        if (materials is null) return;
        if (!this.eyedropper.enabled) return;

        materialUIs
            .Where(x => x)
            .Select(x => x.gameObject)
            .ForEach(Destroy);//foreach (var item in materialUIs) { if (item) Destroy(item.gameObject); }
        materialUIs.Clear();
        materials
            .Select(x =>
                Instantiate(loader.AccessoryListElement, listRoot)
               .AddComponent<ReadOnlyMatUI>()
               .Constructor(x, materialManager, contextMenu))
            .ForEach(materialUIs.Add);
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (this.contextMenu.isActiveAndEnabled) return;

        Log.Debug("Toggling Eyedropper");
        this.eyedropper.enabled = !this.eyedropper.enabled;
    }

}
