using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Contracts;
using CarolCustomizer.Events;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using System;
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
    #endregion

    #region Component Refrerences
    Image background;
    Image displayImage;
    Text displayName;
    Text pickupLocation;
    #endregion

    #region State Variables
    Dictionary<AccessoryDescriptor, AccessoryUI> AccUIs = new();
    public bool expanded = false;
    public Color color = Constants.DefaultColor;
    #endregion

    #region Setup
    public OutfitUI Constructor(Outfit outfit, OutfitListUI ui)
    {
        this.outfit = outfit;
        this.ui = ui;

        name = "OutfitUI: " + outfit.DisplayName;
        background = transform.GetChild(0).gameObject.GetComponent<Image>();
        background.color = color;
        displayImage = transform.Find(displayImageAddress)?.GetComponent<Image>();
        displayImage.sprite = outfit.Sprite;
        displayName = transform.Find(outfitNameAddress)?.GetComponentInChildren<Text>();
        displayName.text = outfit.DisplayName;
        pickupLocation = transform.Find(pickupLocationAddress)?.GetComponent<Text>();
        pickupLocation.text = "";
        outfit.Accessories.ForEach(acc => AccUIs.Add(acc, null));
        return this;
    }
    #endregion

    #region Input Event Handling
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == Settings.HotKeys.ContextMenu) { OnContextClick(); }
        if (eventData.button == PointerEventData.InputButton.Left) { OnLeftClick(); }
    }

    public void Hide()
    {
        Collapse();
        this.gameObject.SetActive(false);
    }

    public void Show() => this.gameObject.SetActive(true);

    public void Expand() => expanded = ShowAccessoriesWhere(x => true);

    void Collapse()
    {
        AccUIs.Values
                .Where(ui => ui)
                .Select(ui => ui.gameObject)
                .ToList()
                .ForEach(go => go.SetActive(false));
        expanded = false;
    }

    AccessoryUI GetAccUI(AccessoryDescriptor descriptor)
    {
        if (descriptor == null) return null;

        if (!AccUIs.TryGetValue(descriptor, out var UI))
        {
            Log.Warning($"Key {descriptor} wasn't in {outfit.DisplayName}.");
            return null;
        }

        if (!UI) UI = AccUIs[descriptor] = ui.Factory.BuildAccUI(this, descriptor);
        return UI;
    }

    void OnContextClick() => ui.ContextMenu.Show(this);
    void OnLeftClick() { if (expanded.Flip()) Expand(); else Collapse(); }

    public void HandleFilterEvent(UIFilterChangedEvent eventData)
    {
        if (!eventData.AnyFilters) { Collapse(); Show(); return; }
        if (!eventData.HasText) { Hide(); return; }

        if (ShowAccessoriesWhere(x =>  FilterUI.CheckAccessory(x, eventData))){ Show(); return; }
        if (FilterUI.CheckOutfit(outfit, eventData)) { Show(); return; }
        Hide();
    }

    public void HandleAccessoryChanged(AccessoryChangedEvent eventData)
    {
        Log.Debug("OutfitUI.HandleAccessoryChanged()");
        var accUI = GetAccUI(eventData.Target);
        if (!accUI) { Log.Error($"didn't find accessory {eventData.Target.DisplayName} for ACE in {outfit.DisplayName}"); return; }

        accUI.HandleAccessoryChanged(eventData);
        OnAccessoryToggled();
    }

    public void SetAccessoryVisible(AccessoryDescriptor accessory)
    {
        var ui = GetAccUI(accessory);
        if (!ui) { Log.Warning("didn't find accessory in outfit"); return; }

        Show();
        ui.gameObject.SetActive(true);
    }

    bool ShowAccessoriesWhere(Predicate<AccessoryDescriptor> predicate)
    {
        return AccUIs.Keys
            .Where(acc => predicate(acc))
            .ToList()
            .Select(GetAccUI)
            .ForEach(ui => ui.gameObject.SetActive(true))
            .Any();
    }

    public void OnAccessoryToggled()
    {
        if (AccUIs.Values
            .Where(x => x)
            .Any(x => x.activationToggle.isOn)) 
        {
            Show();
            background.color = Constants.Highlight; 
            return; 
        }
        background.color = color;
    }

    public void ClearFavorites()
    {
        AccUIs.Values
            .Where(x=>x)
            .ForEach(x=>x.SetFavorite(false));
    }

    public List<(string, UnityAction)> GetContextMenuItems()
    {
        var hads = outfit as HaDSOutfit; //TODO: idk, but not this
        var target = OutfitListUI.TargetOutfit;
        var results = new List<(string, UnityAction)>()
        {
             ("Use Animator",     () => target.SetAnimator(outfit))
            ,("Use Measurements", () => target.SetConfiguration(hads))
            ,("Use Colliders",    () => target.SetColliderSource(outfit))
            ,("Activate Effects", () => target.SetEffect(outfit, true))
            ,("Disable Effects",  () => target.SetEffect(outfit, false))
        };
        hads.Variants.Keys
            .ForEach(x =>
                results.Add(($"Load: {x}", () => 
                    RecipeApplier.ActivateVariant(target, hads, x))));
        return results;
    }
    #endregion

    void OnDestroy() => AccUIs.Values
        .Where(x => x)
        .Select(x => x.gameObject)
        .ForEach(GameObject.Destroy);
}
