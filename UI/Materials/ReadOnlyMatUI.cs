using CarolCustomizer.Behaviors;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models;
using CarolCustomizer.UI.Main;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Materials;
internal class ReadOnlyMatUI : MonoBehaviour, IPointerClickHandler, IContextMenuActions
{
    const string displayNameAddress = "Text/Accessory Name";
    const string materialNameAddress = "Text/Material Name";
    const string favoriteAddress = "Favorite";
    const string toggleAddress = "Toggle";

    Main.ContextMenu contextMenu;
    MaterialManager materialManager;
    MaterialDescriptor material;

    Text displayName;
    Text materialName;
    Image favoriteIcon;

    public void Constructor(MaterialDescriptor material, MaterialManager materialManager, Main.ContextMenu contextMenu)
    {
        this.material = material;
        this.materialManager = materialManager;
        this.contextMenu = contextMenu;

        displayName = transform.Find(displayNameAddress).GetComponent<Text>();
        displayName.text = this.material.Name;

        materialName = transform.Find(materialNameAddress).GetComponent<Text>();
        materialName.text = "";

        var toggle = transform.Find(toggleAddress).gameObject;
        GameObject.Destroy(toggle.GetComponent<Toggle>());
        GameObject.Destroy(toggle);

        favoriteIcon = transform.Find(favoriteAddress)?.GetComponent<Image>();
        favoriteIcon.enabled = false;

        name = "ReadonlyMatUI: " + this.material.Name;
    }

    private void CopyMaterial()
    {
        materialManager.clipboard = material;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == Settings.HotKeys.ContextMenu) { OnContextClick(); return; }
    }

    private void OnContextClick()
    {
        contextMenu.Show(this);
    }

    public List<(string, UnityAction)> GetContextMenuItems()
    {
        List<(string, UnityAction)> menuItems = new()
        {
            ("Copy Material", CopyMaterial),
        };
        return menuItems;
    }
}
