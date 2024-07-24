using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Contracts;
using CarolCustomizer.Events;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Outfits;
/// <summary>
/// Represents one Accessory in the UI. 
/// </summary>
public class AccessoryUI : MonoBehaviour, IPointerClickHandler, IContextMenuActions
{
    #region Static Addresses
    static string displayNameAddress = "Text/Accessory Name";
    static string materialNameAddress = "Text/Material Name";
    static string favoriteAddress = "Favorite";
    #endregion

    #region Dependencies
    OutfitUI outfitUI;
    public StoredAccessory accessory { get; private set; }
    OutfitListUI ui;
    #endregion

    #region Local Component References
    Text displayName;
    Text materialName;
    public Toggle activationToggle { get; private set; }
    Image favoriteIcon;
    #endregion

    #region State Variables
    bool expanded = false;
    #endregion

    #region Lists
    List<MaterialUI> matUIs;
    #endregion

    public string DisplayName => accessory.DisplayName;

    #region Constructor
    public void Constructor(
        OutfitUI outfitUI,
        StoredAccessory accessory,
        OutfitListUI ui)
    {
        this.outfitUI = outfitUI;
        this.accessory = accessory;
        this.ui = ui;
        name = "AccUI: " + accessory.DisplayName;
        displayName = transform
            .Find(displayNameAddress)
            .GetComponent<Text>();
        materialName = transform
            .Find(materialNameAddress)
            .GetComponent<Text>();
        favoriteIcon = transform
            .Find(favoriteAddress)
            .GetComponent<Image>();
        activationToggle = transform
            .GetComponentInChildren<Toggle>(true);
        displayName.text = accessory.DisplayName;
        materialName.text = $"Material Count: {accessory.Materials.Count()}";
        favoriteIcon.enabled = Settings.Favorites.IsInFavorites(accessory);
        activationToggle.onValueChanged
            .AddListener(OnToggle);
        matUIs = accessory.Materials
            .Select((mat, i)=> 
                ui.Factory.BuildMatUI(this, i, mat))
            .ToList();
        gameObject.SetActive(false);
    }
    #endregion

    #region Event Handling
    void OnDisable()
    {
        expanded = false;
        SetMaterialVisibity(expanded);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == Settings.HotKeys.ContextMenu) { OnContextClick(); return; }
        if (eventData.button == PointerEventData.InputButton.Left) { OnLeftClick(); return; }
    }

    void OnLeftClick()
    {
        expanded.Flip();
        SetMaterialVisibity(expanded);
    }

    void SetMaterialVisibity(bool state)
    {
        matUIs.ForEach(x => x.gameObject.SetActive(state));
    }

    public void HandleAccessoryChanged(AccessoryChangedEvent eventData)
    {
        Log.Debug("AccUI.HandleAccessoryChanged");
        this.activationToggle.SetIsOnWithoutNotify(eventData.Visible);
        eventData.State.Materials
            .Zip(matUIs, (x, y) => (des: x, ui: y))
            .ForEach(tup => tup.ui.UpdateActiveMaterial(tup.des));    
    }

    void OnContextClick() => ui.ContextMenu.Show(this);

    public void OnToggle(bool state)
    {
        outfitUI.OnAccessoryToggled();

        if (state) OutfitListUI.TargetOutfit.EnableAccessory(accessory);
        else OutfitListUI.TargetOutfit.DisableAccessory(accessory);
    }

    public void SetFavorite(bool favorited)
    {
        if (favorited) Settings.Favorites.AddToFavorites(accessory); 
        else Settings.Favorites.RemoveFromFavorites(accessory);
        favoriteIcon.enabled = favorited;
    }

    public List<(string, UnityAction)> GetContextMenuItems()
    {
        bool currentlyFavorite = Settings.Favorites.IsInFavorites(accessory);
        return new List<(string, UnityAction)> { 
            (currentlyFavorite ? "Remove from Favorites" : "Add to favorites", 
            () => SetFavorite(!currentlyFavorite)) 
        };
    }
    #endregion
}
