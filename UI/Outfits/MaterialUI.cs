﻿using CarolCustomizer.Contracts;
using Slate.ActionClips;
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
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.UI.Main;

namespace CarolCustomizer.UI.Outfits;
public class MaterialUI : MonoBehaviour, IPointerClickHandler, IContextMenuActions
{
    #region Static Addresses
    static string defaultMaterialTextAddress = "Text/Accessory Name";
    static string currentMaterialTextAddress = "Text/Material Name";
    static string favoriteAddress = "Favorite";
    #endregion

    #region Dependencies
    OutfitListUI ui;
    AccessoryUI accessoryUI;
    MaterialDescriptor defaultMaterial;
    DynamicContextMenu contextMenu;
    OutfitManager outfitManager;

    public int index { get; private set; }
    #endregion

    #region Local Component References
    Text defaultMaterialName;
    Text currentMaterialName;
    Image favoriteIcon;
    RectTransform rect;
    #endregion

    public void Constructor(AccessoryUI accessory, MaterialDescriptor material, int index, OutfitListUI ui, DynamicContextMenu contextMenu, OutfitManager outfitManager)
    {
        accessoryUI = accessory;
        defaultMaterial = material;
        this.outfitManager = outfitManager;
        this.ui = ui;
        this.index = index;
        this.contextMenu = contextMenu;
        rect = GetComponent<RectTransform>();

        rect.Translate(new Vector3(16, 0, 0));
        rect.sizeDelta = new Vector2(208, 32);

        defaultMaterialName = transform.Find(defaultMaterialTextAddress).GetComponent<Text>();
        defaultMaterialName.text = defaultMaterial.Name;

        currentMaterialName = transform.Find(currentMaterialTextAddress).GetComponent<Text>();
        currentMaterialName.text = CurrentLiveMaterial().Name;

        name = "MatUI: " + defaultMaterial.Name;

        favoriteIcon = transform.Find(favoriteAddress)?.GetComponent<Image>();
        favoriteIcon.enabled = false;

        gameObject.SetActive(false);
    }

    public void UpdateActiveMaterial(MaterialDescriptor material)
    {
        if (material is null) material = CurrentLiveMaterial();
        currentMaterialName.text = material.Name;
    }

    private void CopyDefaultMaterial()
    {
        ui.materialManager.clipboard = defaultMaterial;
    }

    private void CopyCurrentMaterial()
    {
        ui.materialManager.clipboard = CurrentLiveMaterial();
    }

    private void PasteMaterial() => SetMaterial(ui.materialManager.clipboard);

    private void SetMaterial(MaterialDescriptor material)
    {
        Log.Debug("MatUI SetMaterial!");
        outfitManager.PaintAccessory(accessoryUI.accessory, material, index);
    }

    private MaterialDescriptor CurrentLiveMaterial()
    {
        var storedAcc = accessoryUI.accessory;
        var mats = outfitManager.GetLiveMaterials(storedAcc);
        if (mats is null) return defaultMaterial;
        return mats[index];
    }

    private void ResetMaterial() => SetMaterial(defaultMaterial);

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == Settings.HotKeys.ContextMenu) { OnContextClick(); return; }
    }

    private void OnContextClick() => contextMenu.Show(this);

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