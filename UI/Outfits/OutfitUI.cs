using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.UI.Main;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Outfits;
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

        name = "OutfitUI: " + outfit.DisplayName;

        background = transform.GetChild(0).gameObject.GetComponent<Image>();
        background.color = Constants.DefaultColor;

        displayImage = transform.Find(displayImageAddress)?.GetComponent<Image>();
        displayImage.sprite = outfit.Sprite;

        displayName = transform.Find(outfitNameAddress)?.GetComponentInChildren<Text>();
        displayName.text = outfit.DisplayName;

        pickupLocation = transform.Find(pickupLocationAddress)?.GetComponent<Text>();
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
        if (eventData.button == Settings.HotKeys.ContextMenu) { OnContextClick(); }
        if (eventData.button == PointerEventData.InputButton.Left) { OnLeftClick(); }
    }

    private void OnLeftClick()
    {
        expanded.Flip();
        ui.SetOutfitExpanded(outfit, expanded);
    }

    private void OnContextClick() => contextMenu.Show(this);

    public void OnAccessoryToggled()
    {
        if (Accessories.Any(x => x.activationToggle.isOn)) { background.color = Constants.Highlight; return; }
        background.color = Constants.DefaultColor;
    }

    public List<(string, UnityAction)> GetContextMenuItems()
    {
        var results = new List<(string, UnityAction)>();
        var hads = outfit as HaDSOutfit; //TODO: idk but anything but this
        results.Add(("Use Animator", () => ui.playerManager.outfitManager.SetAnimator(outfit)));
        foreach (var entry in hads.Variants)
        {
            var idk = () => RecipeApplier.ActivateVariant(ui.playerManager.outfitManager, hads, entry.Key);
            results.Add(($"Load: {entry.Key}", new UnityAction(idk)));
        }
        return results;
    }
    #endregion
}
