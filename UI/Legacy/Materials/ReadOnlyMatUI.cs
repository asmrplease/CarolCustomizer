using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Legacy.Materials;
internal class ReadOnlyMatUI : MonoBehaviour, IPointerClickHandler, IContextMenuActions
{
    const string displayNameAddress = "Text/Accessory Name";
    const string materialNameAddress = "Text/Material Name";
    const string favoriteAddress = "Favorite";
    const string toggleAddress = "Toggle";

    Main.ContextMenu contextMenu;
    MaterialDescriptor material;

    Text displayName;
    Text sceneName;
    Image favoriteIcon;
    Image background;

    public ReadOnlyMatUI Constructor(MaterialDescriptor material, Main.ContextMenu contextMenu)
    {
        this.material = material;
        this.contextMenu = contextMenu;

        displayName = transform.Find(displayNameAddress).GetComponent<Text>();
        displayName.text = this.material.Name.DeInstance();

        sceneName = transform.Find(materialNameAddress).GetComponent<Text>();
        sceneName.text = material.Source.Name;

        var toggle = transform.Find(toggleAddress).gameObject;
        Destroy(toggle.GetComponent<Toggle>());
        Destroy(toggle);

        background = GetComponent<Image>();
        background.color = material.referenceMaterial ? 
            Constants.DefaultColor : 
            Constants.DefaultColor.RGBMultiplied(0.5f);

        favoriteIcon = transform.Find(favoriteAddress)?.GetComponent<Image>();
        favoriteIcon.enabled = false;

        name = "ReadonlyMatUI: " + this.material.Name;
        return this;
    }

    void CopyMaterial()
    {
        MaterialManager.clipboard = material;
        SceneResourceProvider.Cache(material);
    }

    void LoadMaterial()
    {
        string scene = material.Source.Name;
        CCPlugin.CoroutineRunner.StartCoroutine(SceneResourceProvider.BatchQueueAndThen(
            material,
            (_) => { }));
        CCPlugin.CoroutineRunner.StartCoroutine(
            SceneResourceProvider.BatchLoad());
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == Settings.HotKeys.ContextMenu) { OnContextClick(); return; }
    }

    void OnContextClick()
    {
        contextMenu.Show(this);
    }

    public List<ContextButton> GetContextMenuItems()
    {
        List<ContextButton> menuItems = [];
        if (material.referenceMaterial) menuItems.Add(("Copy Material", CopyMaterial));
        else menuItems.Add(("Load Material", LoadMaterial));
        return menuItems;
    }
}
