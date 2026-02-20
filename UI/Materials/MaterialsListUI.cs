using CarolCustomizer.Assets;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.UI.Main;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CarolCustomizer.UI.Materials;
public class MaterialsListUI : MonoBehaviour
{
    private static readonly string listRootAddress = "Scroll View/Viewport/Content";

    UIAssetLoader loader;
    Eyedropper eyedropper;
    Main.ContextMenu contextMenu;

    Transform listRoot;

    readonly List<ReadOnlyMatUI> eyedropperMaterialUIs = [];
    readonly Dictionary<MaterialDescriptor, ReadOnlyMatUI> permanentUIElements = [];

    public MaterialsListUI Constructor(UIAssetLoader loader, Main.ContextMenu contextMenu)
    {
        this.loader = loader;
        this.contextMenu = contextMenu;
        this.eyedropper = this.gameObject.AddComponent<Eyedropper>().Constructor(loader);
        this.eyedropper.OnMaterialsFound += OnEyedropperChanged;
        SceneResourceProvider.SetCallback();
        SceneResourceProvider.OnMaterialLoaded += OnMaterialLoaded;
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
        if (!this.eyedropper) return;

        this.eyedropper.enabled = true;
    }

    public void OnMaterialLoaded(MaterialDescriptor mat)
    {
        Log.Debug($"Creating materialUI for {mat.Name}");
        if (permanentUIElements.TryGetValue(mat, out var existing))
        {
            Destroy(existing.gameObject);
            permanentUIElements.Remove(mat);
        }
        var ui = Instantiate(loader.AccessoryListElement, listRoot)
            .AddComponent<ReadOnlyMatUI>()
            .Constructor(mat, contextMenu);
        permanentUIElements.Add(mat, ui);
    }

    private void OnEyedropperChanged(List<MaterialDescriptor> materials)
    {
        if (materials is null) return;
        if (!this.eyedropper.enabled) return;

        eyedropperMaterialUIs
            .Where(x => x)
            .Select(x => x.gameObject)
            .ForEach(Destroy);
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
        if (!MenuToggle.IsVisible) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Log.Debug("Toggling Eyedropper");
        this.eyedropper.enabled = !this.eyedropper.enabled;
    }
}
