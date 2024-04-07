﻿using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors;
using CarolCustomizer.Events;
using CarolCustomizer.Models;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using static ModelData;

namespace CarolCustomizer.UI;
internal class OutfitListUI : MonoBehaviour
{
    #region Static Constants
    private readonly string SearchModeHint = "Click here to search!";
    private static readonly string searchBoxAddress = "Search Box";
    private static readonly string searchBoxHintAddress = "Search Box/Text";
    private static readonly string listRootAddress = "Scroll View/Viewport/Content";
    private string showHideAddress = "Buttons/Toggle Base";
    private static readonly string uncheckAllAddress = "Buttons/Uncheck All";
    private string filtersAddress = "Filters";
    #endregion

    private TabbedUIAssetLoader loader;
    private OutfitManager outfitManager;
    public CarolInstance playerManager { get; private set; }
    public MaterialManager materialManager { get; private set; }
    public FavoritesManager favoritesManager { get; private set; }
    public HotKeyConfig hotkeys { get; private set; }
    private DynamicContextMenu contextMenu;

    public string searchString { get; private set; }
    private Transform listRoot;
    private InputField searchBox;
    private Text searchBoxHint;
    private Toggle favoriteFilter;
    private Toggle activeFilter;

    private Button deselectAll;
    private Button showHideBase;

    private SortedList<Outfit, OutfitUI> outfitUIs = new();
    private Dictionary<StoredAccessory, AccessoryUI> accessoryUIs = new();
    private Dictionary<AccMatSlot, MaterialUI> materialUIs = new();

    public void Constructor(
        TabbedUIAssetLoader loader, 
        CarolInstance playerManager,
        MaterialManager materialManager, 
        FavoritesManager favoritesManager, 
        HotKeyConfig hotkeys,
        DynamicContextMenu contextMenu)
    {
        this.loader = loader;
        this.materialManager = materialManager;
        this.playerManager = playerManager;
        this.outfitManager = playerManager.outfitManager;
        this.favoritesManager = favoritesManager;
        this.hotkeys = hotkeys;
        this.contextMenu = contextMenu;

        OutfitAssetManager.OnOutfitLoaded += OnOutfitLoaded;
        OutfitAssetManager.OnOutfitUnloaded += OnOutfitUnloaded;

        outfitManager.AccessoryChanged += HandleAccessoryChanged;

        this.searchString = "";

        deselectAll = this.transform.Find(uncheckAllAddress).GetComponent<Button>();
        deselectAll.onClick.AddListener(OnDisableAll);
        favoriteFilter = this.transform.Find(filtersAddress + "/Favorites").GetComponent<Toggle>();
        favoriteFilter.onValueChanged.AddListener(ProcessFilters);
        activeFilter = this.transform.Find(filtersAddress + "/Active").GetComponent<Toggle>();
        activeFilter.onValueChanged.AddListener(ProcessFilters);
        showHideBase = this.transform.Find(showHideAddress).GetComponent<Button>();
        showHideBase.onClick.AddListener(outfitManager.ToggleBaseVisibility);

        listRoot = this.transform.Find(listRootAddress);

        searchBox = this.transform.Find(searchBoxAddress).GetComponent<InputField>();
        searchBox.onEndEdit.AddListener(OnSearchBoxChanged);

        searchBoxHint = searchBox.transform.GetChild(1).GetComponent<Text>();
        searchBoxHint.text = SearchModeHint;
    }

    private void OnDestroy()
    {
        OutfitAssetManager.OnOutfitLoaded -= OnOutfitLoaded;
        OutfitAssetManager.OnOutfitUnloaded -= OnOutfitUnloaded;
        outfitManager.AccessoryChanged -= HandleAccessoryChanged;
    }

    private void OnSearchBoxChanged(string searchString)
    {
        this.searchString = searchString;
        ProcessFilters();
    }

    private void ProcessFilters(bool unused = true)
    {
        bool favorites = favoriteFilter.isOn;
        bool active = activeFilter.isOn;
        bool textFilter = searchString.Trim() != "";

        bool accFilterActive = favorites || active || textFilter;

        foreach (var outfit in outfitUIs)
        {
            if (searchString == "") { outfit.Value.gameObject.SetActive(!accFilterActive); continue; }
            bool textSearchResult = outfit.Key.DisplayName.ToLower().Contains(searchString.ToLower());
            outfit.Value.gameObject.SetActive(textSearchResult);
        }

        foreach (var acc in accessoryUIs.Values)
        {
            if (!acc) continue;
            if (favorites && favoritesManager.IsInFavorites(acc.accessory)) { acc.ShowAndExpandOutfit(); continue; }
            if (active && outfitManager.IsEnabled(acc.accessory)) { acc.ShowAndExpandOutfit(); continue; }

            if (searchString == "") { acc.gameObject.SetActive(false); continue; }
            bool textSearchResult = acc.DisplayName.ToLower().Contains(searchString.ToLower());
            if (textSearchResult) { acc.ShowAndExpandOutfit(); continue; }

            acc.gameObject.SetActive(false);
        }
    }

    private void NewProcessFilters(bool unused)
    {
        //hide all ui elements
        //show ui elements that match a filter
    }

    private void OnOutfitLoaded(Outfit outfit)
    {
        var uiInstance = GameObject.Instantiate(loader.OutfitListElement, listRoot);
        if (!uiInstance) { Log.Error("Failed to instantiate outfit UI prefab."); return; }

        var outfitUI = uiInstance.AddComponent<OutfitUI>();
        if (!outfitUI) { Log.Error("Failed to add OutfitUI component"); return; }
        outfitUI.Constructor(outfit, this, contextMenu);

        outfitUIs[outfit] = outfitUI;
        outfitUI.transform.SetSiblingIndex(outfitUIs.IndexOfKey(outfit));
        
        foreach (var accessory in outfit.Accessories)
        {
            var accElement = OnAccessoryLoaded(outfitUI, accessory);
            outfitUI.AddAccessory(accElement);
            accessoryUIs.Add(accessory, accElement);
        }
        
    }

    private void OnOutfitUnloaded(Outfit outfit)
    {
        Log.Debug($"UI.OnOutfitUnloaded({outfit.DisplayName})");
        //find the associated UI element
        if (!outfitUIs.ContainsKey(outfit)) { Log.Warning("UI instance was asked to remove a UI element that was not in it's dict"); return; }
        var outfitUI = outfitUIs[outfit];
        //destroy it
        outfitUIs.Remove(outfit);
        GameObject.Destroy(outfitUI.gameObject);
    }

    private AccessoryUI BuildAccUI(StoredAccessory accessory)
    {
        var outfitUI = outfitUIs[accessory.outfit];

        var accInstance = GameObject.Instantiate(loader.AccessoryListElement, outfitUI.transform);
        if (!accInstance) { Log.Error("Failed to instantiate accessory UI prefab."); return null; }

        var accUI = accInstance.AddComponent<AccessoryUI>();
        if (!accUI) { Log.Error("Failed to add AccUI component"); return null; }
        accUI.Constructor(outfitUI, accessory, this, contextMenu, outfitManager, favoritesManager);
        accessoryUIs[accessory] = accUI;
        return accUI;
    }

    private MaterialUI BuildMatUI(AccMatSlot location, MaterialDescriptor material) 
    {
        var accessoryUI = accessoryUIs[location.accessory];
        if (!accessoryUI) 
        {
            accessoryUI = BuildAccUI(location.accessory);
        }
        var matInstance = GameObject.Instantiate(loader.AccessoryListElement, accessoryUI.transform.parent);
        if (!matInstance) { Log.Error("Failed to instantiate material UI prefab."); return null; }

        int index = accessoryUI.transform.GetSiblingIndex() + location.index + 1;
        matInstance.transform.SetSiblingIndex(index);

        var matUI = matInstance.AddComponent<MaterialUI>();
        if (!matUI) { Log.Error("Failed to add MatUI component"); return null; }

        matUI.Constructor(accessoryUI, material, location.index, this, contextMenu, outfitManager);
        accessoryUI.AddMaterial(matUI);
        materialUIs[location] = matUI;
        return matUI;
    }

    public AccessoryUI OnAccessoryLoaded(OutfitUI outfitUI, StoredAccessory accessory)
    {
        int index = 0;
        foreach (var mat in accessory.Materials)
        {
            var matUI = OnMaterialLoaded(accessory, mat, index++);
        }
        return null;
    }

    public MaterialUI OnMaterialLoaded(StoredAccessory accessory, MaterialDescriptor material, int index)
    {
        
        materialUIs.Add(new AccMatSlot { accessory = accessory, index = index }, null);
        return null;
    }


    public void SetOutfitExpanded(Outfit outfit, bool expanded)
    {
        foreach (StoredAccessory accessory in outfit.Accessories)
        {
            SetAccUIVisible(accessory, expanded);
        }
    }

    public void SetAccessoryExpanded(StoredAccessory accessory, bool expanded)
    {
        int index = 0;
        foreach (var mat in accessory.Materials)
        {
            SetMatVisible(new AccMatSlot { accessory = accessory, index = index }, expanded);
        }
    }

    private void SetMatVisible(AccMatSlot material, bool visible)
    {
        var uiInstance = materialUIs[material];
        if (!visible) { uiInstance?.gameObject.SetActive(false); return; }
        if (!uiInstance)
        {
            uiInstance = BuildMatUI(material, material.accessory.Materials[material.index]);
            if (!uiInstance) { Log.Error($"Failed to find or construct {material}'s UI instance during show"); return; }
            materialUIs[material] = uiInstance;
        }
        uiInstance.gameObject.SetActive(true);
    }

    private void SetAccUIVisible(StoredAccessory accessory, bool visible)
    {
        var uiInstance = accessoryUIs[accessory];
        if (!visible) { uiInstance?.gameObject.SetActive(false); return; }

        if (!uiInstance)
        {
            uiInstance = BuildAccUI(accessory);
            if (!uiInstance) { Log.Error($"Failed to find or construct {accessory}'s UI instance during show"); return; }
        }
        uiInstance.gameObject.SetActive(true);
    }


    internal void SetBaseOutfit(Outfit outfit)
    {
        outfitManager.SetBaseOutfit(outfit);
    }

    private void OnDisableAll()
    {
        outfitManager.DisableAllAccessories();
        foreach (var accUI in accessoryUIs.Values)
        {
            accUI.activationToggle.SetIsOnWithoutNotify(false);
        }
    }

    private record AccMatSlot { public StoredAccessory accessory; public int index; }

    private void HandleAccessoryChanged(AccessoryChangedEvent e)
    {
        Log.Debug("OutfitListUI.OnAccessoryStateChanged");
        if (!accessoryUIs.ContainsKey(e.Target)) { Log.Error($"Key {e.Target} was not found in dict."); return; }

        if (!accessoryUIs[e.Target]) BuildAccUI(e.Target);
        accessoryUIs[e.Target].activationToggle.SetIsOnWithoutNotify(e.Visible);
        //accessoryUIs[e.Target].activationToggle.isOn = e.Visible;

        int index = 0;
        foreach (var materialState in e.State.Materials)
        {
            var accMatSlot = new AccMatSlot { accessory = e.Target, index = index};
            if (!materialUIs[accMatSlot]) BuildMatUI(accMatSlot, e.Target.Materials[index]);
            materialUIs[accMatSlot].UpdateActiveMaterial(materialState);
            index++;
        }
        
    }
}
