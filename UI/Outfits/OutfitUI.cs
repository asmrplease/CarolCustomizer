using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Outfits;
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
    #region Addresses
    static readonly string displayImageAddress = "Outfit Header/Icon";
    static readonly string outfitNameAddress = "Outfit Header/Text/Outfit Name";
    static readonly string pickupLocationAddress = "Outfit Header/Text/Pickup Location";
    #endregion

    #region Dependencies
    public Outfit outfit { get; private set; }
    OutfitListUI ui;
    Main.ContextMenu contextMenu;
    #endregion

    #region Component Refrerences
    Image background;
    Image displayImage;
    Text displayName;
    Text pickupLocation;
    #endregion

    #region State Variables
    List<AccessoryUI> Accessories = new();
    public bool expanded = false;
    #endregion

    #region Setup
    public void Constructor(Outfit outfit, OutfitListUI ui, Main.ContextMenu contextMenu)
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

    void OnLeftClick()
    {
        expanded.Flip();
        ui.SetOutfitExpanded(outfit, expanded);
    }

    void OnContextClick() => contextMenu.Show(this);

    public void OnAccessoryToggled()
    {
        if (Accessories.Any(x => x.activationToggle.isOn)) { background.color = Constants.Highlight; return; }
        background.color = Constants.DefaultColor;
    }

    public List<(string, UnityAction)> GetContextMenuItems()
    {
        var hads = outfit as HaDSOutfit; //TODO: idk, but not this
        var results = new List<(string, UnityAction)>()
        {
             ("Use Animator",     () => ui.playerManager.outfitManager.SetAnimator(outfit))
            ,("Use Measurements", () => ui.playerManager.outfitManager.SetConfiguration(hads))
            ,("Activate Effects", () => ui.playerManager.outfitManager.SetEffects(outfit, true))
            ,("Disable Effects",  () => ui.playerManager.outfitManager.SetEffects(outfit, false))
        };

        foreach (var entry in hads.Variants)
        {
            results.Add(
                ($"Load: {entry.Key}", () => RecipeApplier.ActivateVariant(ui.playerManager.outfitManager, hads, entry.Key)));
        }
        return results;
    }
    #endregion

    void OnDestroy()
    {
        foreach(var accUI in Accessories)
        {
            ui.OnAccessoryUnloaded(accUI.accessory);
            GameObject.Destroy(accUI.gameObject);
        }
    }
}
