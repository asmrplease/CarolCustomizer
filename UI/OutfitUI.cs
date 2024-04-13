using CarolCustomizer.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ModelData;
using CarolCustomizer.Models;
using CarolCustomizer.Utils;
using CarolCustomizer.Behaviors;

namespace CarolCustomizer.UI;
public class OutfitUI : MonoBehaviour, IPointerClickHandler, IContextMenuActions
{

    #region Dependencies
    public Outfit outfit { get; private set; }
    OutfitListUI ui;
    DynamicContextMenu contextMenu;
    #endregion

    #region Component Refrerences
    private Image background;
    private Image displayImage;
    private Text displayName;
    private Text pickupLocation;
    #endregion

    #region Addresses
    static readonly string displayImageAddress = "Outfit Header/Icon";
    static readonly string outfitNameAddress = "Outfit Header/Text/Outfit Name";
    static readonly string pickupLocationAddress = "Outfit Header/Text/Pickup Location";
    #endregion

    #region State Variables
    private List<AccessoryUI> Accessories = new();
    public bool expanded = false;
    #endregion

    #region Setup
    public void Constructor(Outfit outfit, OutfitListUI ui, DynamicContextMenu contextMenu)
    {
        this.outfit = outfit;
        this.ui = ui;
        this.contextMenu = contextMenu;

        this.name = "OutfitUI: " + outfit.DisplayName;

        background = this.transform.GetChild(0).gameObject.GetComponent<Image>();
        background.color = Constants.DefaultColor;

        displayImage = this.transform.Find(displayImageAddress)?.GetComponent<Image>();
        displayImage.sprite = outfit.Sprite;

        displayName = this.transform.Find(outfitNameAddress)?.GetComponentInChildren<Text>();
        displayName.text = outfit.DisplayName;

        pickupLocation = this.transform.Find(pickupLocationAddress)?.GetComponent<Text>();
        pickupLocation.text = "";

    }
    public void AddAccessory(AccessoryUI accessory)
    {
        if (!accessory) { return; }
        Accessories.Add(accessory);
    }

    #endregion

    #region Input Event Handling
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == ui.hotkeys.ContextMenuMouseButton) { OnContextClick(); }
        if (eventData.button == PointerEventData.InputButton.Left) { OnLeftClick(); }  
    }

    private void OnLeftClick()
    {
        expanded.Flip();
        ui.SetOutfitExpanded(this.outfit, expanded);
    }

    private void OnContextClick() => contextMenu.Show(this);

    public void OnAccessoryToggled()
    {
        if (Accessories.Any(x => x.activationToggle.isOn)) { background.color = Constants.Highlight; return; }
        background.color = Constants.DefaultColor;
    }



    public List<(string, UnityAction)> GetContextMenuItems()
    {
        //return new List<(string, UnityAction)> { ( "Set Outfit", () => ui.SetBaseOutfit(this.outfit) ) };
        var results = new List<(string, UnityAction)>();
        var hads = outfit as HaDSOutfit; //TODO: idk but anything but this
        foreach (var entry in hads.modelData.accessories)
        {
            var idk = () => RecipeApplier.ActivateVariant(ui.playerManager.outfitManager, hads, entry.name);
            results.Add(($"Load: {entry.name}", new UnityAction(idk)));
        }
        return results;
    }
    #endregion
}
