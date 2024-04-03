using CarolCustomizer.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CarolCustomizer.Models;
using CarolCustomizer.Utils;
using CarolCustomizer.Behaviors;

namespace CarolCustomizer.UI;
/// <summary>
/// Represents one Accessory in the UI. 
/// </summary>
internal class AccessoryUI : MonoBehaviour, IPointerClickHandler, IContextMenuActions
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
    private DynamicContextMenu contextMenu;
    private OutfitManager outfitManager;
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
    private List<MaterialUI> materials = new();
    #endregion

    public string DisplayName => accessory.DisplayName;

    #region Constructor
    public void Constructor(OutfitUI outfitUI, StoredAccessory accessory, OutfitListUI ui, DynamicContextMenu contextMenu, OutfitManager outfitManager)
    {
        this.outfitUI = outfitUI;
        this.accessory = accessory;
        this.contextMenu = contextMenu;
        this.ui = ui;
        this.outfitManager = outfitManager;

        this.name = "AccUI: " + accessory.DisplayName;

        displayName = this.transform.Find(displayNameAddress).GetComponent<Text>();
        displayName.text = accessory.DisplayName;

        materialName = this.transform.Find(materialNameAddress).GetComponent<Text>();
        materialName.text = "";

        activationToggle = this.transform.GetComponentInChildren<Toggle>(true);
        activationToggle.onValueChanged.AddListener(OnToggle); 

        favoriteIcon = this.transform.Find(favoriteAddress)?.GetComponent<Image>();
        favoriteIcon.enabled = ui.favoritesManager.IsInFavorites(accessory);

        this.gameObject.SetActive(false);
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
        if (eventData.button == ui.hotkeys.ContextMenuMouseButton) { OnContextClick(); return; }
        if (eventData.button == PointerEventData.InputButton.Left) { OnLeftClick(); return; }
    }

    private void OnLeftClick()
    {
        expanded.Flip();
        SetMaterialVisibity(expanded);
    }

    public void ShowAndExpandOutfit()
    {
        gameObject.SetActive(true);
        outfitUI.gameObject.SetActive(true);
    }

    private void SetMaterialVisibity(bool state)
    {
        ui.SetAccessoryExpanded(this.accessory, state);
    }

    private void OnContextClick() => contextMenu.Show(this);

    public void OnToggle(bool state)
    {
        outfitUI.OnAccessoryToggled();

        if (state) outfitManager.EnableAccessory(accessory);
        else outfitManager.DisableAccessory(accessory);
    }

    private void ToggleFavorite()
    {
        ui.favoritesManager.ToggleFavorite(new AccessoryDescriptor(this.accessory));
        favoriteIcon.enabled = !favoriteIcon.enabled;
    }

    public Dictionary<string, UnityAction> GetContextMenuItems()
    {
        bool currentlyFavorite = ui.favoritesManager.IsInFavorites(this.accessory);
        return new Dictionary<string, UnityAction> { {currentlyFavorite? "Remove from Favorites" : "Add to favorites", ToggleFavorite} };
    }
    #endregion
}
