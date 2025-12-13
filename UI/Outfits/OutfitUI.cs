using CarolCustomizer.Assets;
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
        displayImage.sprite = outfit.Thumbnail;
        displayName = transform.Find(outfitNameAddress)?.GetComponentInChildren<Text>();
        displayName.text = outfit.DisplayName;
        pickupLocation = transform.Find(pickupLocationAddress)?.GetComponent<Text>();
        pickupLocation.text = "";
        outfit.GetAccessories().ForEach(acc => AccUIs.Add(acc, null));
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

    public void Expand() => expanded = ShowAccessoriesWhere(_ => true);

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
            if (OutfitAssetManager.GetInstantiable(descriptor) is not StoredAccessory stored)
            {
                Log.Warning($"Key {descriptor} wasn't in {outfit.DisplayName}.");
                return null;
            }
            if (!AccUIs.TryGetValue(stored, out UI)) { Log.Warning("failed twice to find accUI, good job."); return null; }
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
        if (!accUI) { Log.Error($"OutfitUI.HandleAccessoryChanged: no target {eventData.Target} in {outfit.Descriptor}"); return; }

        accUI.HandleAccessoryChanged(eventData);
        OnAccessoryToggled();
    }

    public void SetAccUIVisible(AccessoryDescriptor accessory)
    {
        var ui = GetAccUI(accessory);
        if (!ui) { Log.Warning("didn't find accessory in outfit"); return; }

        Show();
        ui.gameObject.SetActive(true);
        Log.Debug($"SetAccUIVisible({accessory})");
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
            .ForEach(x=>x.accessory.SetFavorite(false));
    }

    public List<(string, UnityAction)> GetContextMenuItems()
    {
        var hads = outfit as HaDSOutfit; //TODO: idk, but not this
        var target = OutfitListUI.TargetOutfit;
        var results = new List<(string, UnityAction)>()
        {
             ("Use Animator",     () => target.SetAnimator(outfit.Descriptor))
            ,("Use Measurements", () => target.SetConfiguration(outfit.Descriptor))
            ,("Use Colliders",    () => target.SetColliderSource(outfit.Descriptor))
            ,("Activate Effects", () => target.SetEffect(outfit.Descriptor, true))
            ,("Disable Effects",  () => target.SetEffect(outfit.Descriptor, false))
        };
        hads.Variants
            .ForEach(tup =>
                results.Add(($"Load: {tup.Key}", () => RecipeApplier.ActivateRecipe(target, tup.Value.Descriptor))));
        return results;
    }
    #endregion

    //void OnDestroy() => AccUIs.Values
    //    .Where(x => x)
    //    .Select(x => x.gameObject)
    //    .ForEach(GameObject.Destroy);
}
