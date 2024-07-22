using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Contracts;
using CarolCustomizer.Events;
using CarolCustomizer.Models.Accessories;
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
    void OnLeftClick() => SetAccessoriesVisible(expanded.Flip());

    public void HandleFilterEvent(UIFilterChangedEvent eventData)
    {

    }

    public void HandleAccessoryChanged(AccessoryChangedEvent eventData)
    {
        Log.Debug("OutfitUI.HandleAccessoryChanged()");
        var accUI = GetAccUI(eventData.Target);
        if (!accUI) { Log.Error($"didn't find accessory {eventData.Target.DisplayName} for ACE in {outfit.DisplayName}"); return; }

        accUI.HandleAccessoryChanged(eventData);
    }

    void SetAccessoriesVisible(bool visible)
    {
        Log.Debug("SetAccessoriesVisible");
        if (!visible)
        {
            Log.Debug("not visible");
            AccUIs.Values
                .Where(ui => ui)
                .Select(ui => ui.gameObject)
                .ToList()
                .ForEach(go => go.SetActive(false));
        }
        else
        {
            Log.Debug("Visible");
            AccUIs.Keys
                .ToList()
                .Select(GetAccUI)
                .Select (ui => ui.gameObject)
                .ForEach(go => go.SetActive(true));
        }
        OnAccessoryToggled();
        Log.Debug("SetAccVisisble complete;");
    }

    public void OnAccessoryToggled()
    {
        Log.Debug("OnAccessoryToggled()");
        if (AccUIs.Values
            .Where(x => x)
            .Any(x => x.activationToggle.isOn)) 
        { 
            background.color = Constants.Highlight; 
            return; 
        }
        background.color = color;
    }

    public List<(string, UnityAction)> GetContextMenuItems()
    {
        var hads = outfit as HaDSOutfit; //TODO: idk, but not this
        var results = new List<(string, UnityAction)>()
        {
             ("Use Animator",     () => ui.TargetOutfit.SetAnimator(outfit))
            ,("Use Measurements", () => ui.TargetOutfit.SetConfiguration(hads))
            ,("Use Colliders",    () => ui.TargetOutfit.SetColliderSource(outfit))
            ,("Activate Effects", () => ui.TargetOutfit.SetEffect(outfit, true))
            ,("Disable Effects",  () => ui.TargetOutfit.SetEffect(outfit, false))
        };

        foreach (var entry in hads.Variants)
        {
            results.Add(
                ($"Load: {entry.Key}", () => RecipeApplier.ActivateVariant(ui.TargetOutfit, hads, entry.Key)));

        }
        return results;
    }
    #endregion

    void OnDestroy() => AccUIs.Values
        .Where(x => x)
        .Select(x => x.gameObject)
        .ForEach(GameObject.Destroy);
}
