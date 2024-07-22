using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Contracts;
using CarolCustomizer.Events;
using CarolCustomizer.Models.Accessories;
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
    List<MaterialUI> materials = new();
    #endregion

    public string DisplayName => accessory.DisplayName;

    #region Constructor
    public void Constructor(
        OutfitUI outfitUI,
        StoredAccessory accessory,
        OutfitListUI ui
        )
    {
        this.outfitUI = outfitUI;
        this.accessory = accessory;
        this.ui = ui;

        name = "AccUI: " + accessory.DisplayName;

        displayName = transform.Find(displayNameAddress).GetComponent<Text>();
        displayName.text = accessory.DisplayName;

        materialName = transform.Find(materialNameAddress).GetComponent<Text>();
        materialName.text = "";

        activationToggle = transform.GetComponentInChildren<Toggle>(true);
        activationToggle.onValueChanged.AddListener(OnToggle);

        favoriteIcon = transform.Find(favoriteAddress)?.GetComponent<Image>();
        favoriteIcon.enabled = Settings.Favorites.IsInFavorites(accessory);

        gameObject.SetActive(false);
    }

    public void AddMaterial(MaterialUI material)
    {
        materials.Add(material);
        materialName.text = $"Material Count: {materials.Count()}";
    }
    #endregion

    #region Event Handling
    public void OnDisable()
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
        //outfitUI.SetAccessoryExpanded(accessory, state);
    }

    public void HandleAccessoryChanged(AccessoryChangedEvent eventData)
    {
        Log.Debug("AccUI.HandleAccessoryChanged");
        this.activationToggle.SetIsOnWithoutNotify(eventData.Visible);
        //Set material
    }

    void OnContextClick() => ui.ContextMenu.Show(this);

    public void OnToggle(bool state)
    {
        outfitUI.OnAccessoryToggled();

        if (state) ui.TargetOutfit.EnableAccessory(accessory);
        else ui.TargetOutfit.DisableAccessory(accessory);
    }

    public void SetFavorite(bool favorited)
    {
        var description = new AccessoryDescriptor(accessory);//TODO: idr why we can't just pass the existing AD
        if (favorited) Settings.Favorites.AddToFavorites(description); 
        else Settings.Favorites.RemoveFromFavorites(description);
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
