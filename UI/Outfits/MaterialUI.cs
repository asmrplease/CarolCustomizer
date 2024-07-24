using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Outfits;
public class MaterialUI : MonoBehaviour, IPointerClickHandler, IContextMenuActions
{
    #region Static Addresses
    static string defaultMaterialTextAddress = "Text/Accessory Name";
    static string currentMaterialTextAddress = "Text/Material Name";
    static string toggleAddress = "Toggle";

    static string favoriteAddress = "Favorite";
    #endregion

    #region Dependencies
    OutfitListUI ui;
    AccessoryUI accessoryUI;
    MaterialDescriptor defaultMaterial;

    public int index { get; private set; }
    #endregion

    #region Local Component References
    Text defaultMaterialName;
    Text currentMaterialName;
    Image favoriteIcon;
    RectTransform rect;
    #endregion

    public void Constructor(
        AccessoryUI accessory, 
        MaterialDescriptor material, 
        int index, 
        OutfitListUI ui)
    {
        accessoryUI = accessory;
        defaultMaterial = material;
        this.ui = ui;
        this.index = index;
        var toggle = transform.Find(toggleAddress).gameObject;
        GameObject.Destroy(toggle.GetComponent<Toggle>());//TODO: do we need this line???
        GameObject.Destroy(toggle);
        rect = GetComponent<RectTransform>();
        rect.Translate(new Vector3(16, 0, 0));
        rect.sizeDelta = new Vector2(208, 32);
        defaultMaterialName = transform.Find(defaultMaterialTextAddress).GetComponent<Text>();
        defaultMaterialName.text = defaultMaterial.Name;
        currentMaterialName = transform.Find(currentMaterialTextAddress).GetComponent<Text>();
        currentMaterialName.text = CurrentMaterial.Name;
        name = "MatUI: " + defaultMaterial.Name;
        favoriteIcon = transform.Find(favoriteAddress)?.GetComponent<Image>();
        favoriteIcon.enabled = false;
        gameObject.SetActive(false);
    }

    public void UpdateActiveMaterial(MaterialDescriptor material = null)
    {
        material ??= CurrentMaterial;
        currentMaterialName.text = material.Name;
    }

    void CopyDefaultMaterial() => ui.MaterialManager.clipboard = defaultMaterial;

    void CopyCurrentMaterial() => ui.MaterialManager.clipboard = CurrentMaterial;

    void PasteMaterial() => SetMaterial(ui.MaterialManager.clipboard);

    void SetMaterial(MaterialDescriptor material)
    {
        Log.Debug("MatUI SetMaterial!");
        OutfitListUI.TargetOutfit.EnableAccessory(accessoryUI.accessory);
        OutfitListUI.TargetOutfit.PaintAccessory(accessoryUI.accessory, material, index);
    }

    MaterialDescriptor CurrentMaterial =>
        OutfitListUI.TargetOutfit
        .GetLiveMaterials(accessoryUI.accessory)
        ?[index] ?? defaultMaterial;

    void ResetMaterial() => SetMaterial(defaultMaterial);

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == Settings.HotKeys.ContextMenu) { OnContextClick(); return; }
    }

    void OnContextClick() => ui.ContextMenu.Show(this);

    public List<(string, UnityAction)> GetContextMenuItems()
    {
        List<(string, UnityAction)> menuItems = new()
        {
            ("Copy Default Material", CopyDefaultMaterial),
            ("Copy Current Material", CopyCurrentMaterial),
            ("Paste Material", PasteMaterial),
            ("Reset Material", ResetMaterial)
        };
        return menuItems;
    }
}
