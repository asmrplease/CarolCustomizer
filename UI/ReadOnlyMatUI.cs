using CarolCustomizer.Behaviors;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CarolCustomizer.UI;
internal class ReadOnlyMatUI : MonoBehaviour, IPointerClickHandler, IContextMenuActions
{
    private static readonly string displayNameAddress = "Text/Accessory Name";
    private static readonly string materialNameAddress = "Text/Material Name";
    private static readonly string favoriteAddress = "Favorite";

    DynamicContextMenu contextMenu;
    MaterialManager materialManager;
    MaterialDescriptor material;
    HotKeyConfig hotkeys;

    Text displayName;
    Text materialName;
    Image favoriteIcon;

    public void Constructor(MaterialDescriptor material, MaterialManager materialManager, DynamicContextMenu contextMenu, HotKeyConfig hotkeys)
    {
        this.material = material;
        this.materialManager = materialManager;
        this.contextMenu = contextMenu;
        this.hotkeys = hotkeys;

        displayName = this.transform.Find(displayNameAddress).GetComponent<Text>();
        displayName.text = this.material.Name;

        materialName = this.transform.Find(materialNameAddress).GetComponent<Text>();
        materialName.text = "";

        favoriteIcon = this.transform.Find(favoriteAddress)?.GetComponent<Image>();
        favoriteIcon.enabled = false;

        this.name = "ReadonlyMatUI: " + this.material.Name;
    }

    private void CopyMaterial()
    {
        materialManager.clipboard = this.material;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == hotkeys.ContextMenuMouseButton) { OnContextClick(); return; }
    }

    private void OnContextClick()
    {
        contextMenu.Show(this);
    }

    public Dictionary<string, UnityAction> GetContextMenuItems()
    {
        Dictionary<string, UnityAction> menuItems = new()
        {
            { "Copy Material", CopyMaterial},

        };
        return menuItems;
    }
}
