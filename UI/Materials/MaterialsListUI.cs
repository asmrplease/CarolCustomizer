using CarolCustomizer.Assets;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.UI.Main;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.UI.Materials;
internal class MaterialsListUI : MonoBehaviour
{
    private static readonly string listRootAddress = "Scroll View/Viewport/Content";

    UIAssetLoader loader;
    Eyedropper eyedropper;
    Main.ContextMenu contextMenu;

    Transform listRoot;

    readonly List<ReadOnlyMatUI> eyedropperMaterialUIs = [];
    readonly Dictionary<MaterialDescriptor, ReadOnlyMatUI> bookmarkedMaterialUIs = [];

    public MaterialsListUI Constructor(UIAssetLoader loader, Main.ContextMenu contextMenu)
    {
        this.loader = loader;
        this.contextMenu = contextMenu;
        this.eyedropper = this.gameObject.AddComponent<Eyedropper>().Constructor(loader);
        this.eyedropper.OnMaterialsFound += OnEyedropperChanged;
        SceneResourceProvider.SetCallback();
        SceneResourceProvider.OnMaterialLoaded += OnMaterialBookmarked;
        MenuToggle.OnMenuToggle += HandleMenuToggle;
        listRoot = transform.Find(listRootAddress);
        return this;
    }

    void HandleMenuToggle(bool visible)
    {
        this.eyedropper.enabled = visible;
    }

    void OnEnable()
    {
        this.eyedropper.enabled = true;
    }

    private void OnMaterialBookmarked(MaterialDescriptor mat)
    {
        Log.Debug($"Creating materialUI for {mat.Name}");
        if (bookmarkedMaterialUIs.TryGetValue(mat, out var existing))
        {
            Destroy(existing.gameObject);
            bookmarkedMaterialUIs.Remove(mat);
        }
        var ui = Instantiate(loader.AccessoryListElement, listRoot)
            .AddComponent<ReadOnlyMatUI>()
            .Constructor(mat, contextMenu);
        bookmarkedMaterialUIs.Add(mat, ui);
        //ui.transform.SetSiblingIndex(bookmarkedMaterialUIs.IndexOfKey(mat));
    }

    private void OnEyedropperChanged(List<MaterialDescriptor> materials)
    {
        if (materials is null) return;
        if (!this.eyedropper.enabled) return;

        eyedropperMaterialUIs
            .Where(x => x)
            .Select(x => x.gameObject)
            .ForEach(Destroy);//foreach (var item in materialUIs) { if (item) Destroy(item.gameObject); }
        eyedropperMaterialUIs.Clear();
        materials
            .Select(x =>
                Instantiate(loader.AccessoryListElement, listRoot)
               .AddComponent<ReadOnlyMatUI>()
               .Constructor(x, contextMenu))
            .ForEach(eyedropperMaterialUIs.Add);
        eyedropperMaterialUIs.ForEach(x=> x.transform.SetAsFirstSibling());
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (this.contextMenu.isActiveAndEnabled) return;

        Log.Debug("Toggling Eyedropper");
        this.eyedropper.enabled = !this.eyedropper.enabled;
    }

}
