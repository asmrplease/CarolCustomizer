using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Events;
using CarolCustomizer.Models;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.UI.Main;
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

namespace CarolCustomizer.UI.Outfits;
public class OutfitListUI : MonoBehaviour
{
    #region Static Constants
    private readonly string SearchModeHint = "Click here to search!";
    private static readonly string searchBoxAddress = "Search Box";
    private static readonly string searchBoxHintAddress = "Search Box/Text";
    private static readonly string listRootAddress = "Scroll View/Viewport/Content";
    string showHideAddress = "Buttons/Toggle Base";
    private static readonly string uncheckAllAddress = "Buttons/Uncheck All";
    private string filtersAddress = "Filters";
    #endregion

    private TabbedUIAssetLoader loader;
    private OutfitManager outfitManager;
    public CarolInstance playerManager { get; private set; }
    public MaterialManager materialManager { get; private set; }

    private DynamicContextMenu contextMenu;

    public string searchString { get; private set; } = "";
    private Transform listRoot;
    private InputField searchBox;
    private Text searchBoxHint;
    private Toggle favoriteFilter;
    private Toggle activeFilter;

    Button deselectAll;
    Button showHideBase;

    SortedList<Outfit, OutfitUI> outfitUIs = new();
    Dictionary<AccessoryDescriptor, AccessoryUI> accessoryUIs = new();
    Dictionary<AccMatSlot, MaterialUI> materialUIs = new();

    public void Constructor(
        TabbedUIAssetLoader loader,
        CarolInstance playerManager,
        MaterialManager materialManager,
        DynamicContextMenu contextMenu)
    {
        this.loader = loader;
        this.materialManager = materialManager;
        this.playerManager = playerManager;
        outfitManager = playerManager.outfitManager;
        this.contextMenu = contextMenu;

        OutfitAssetManager.OnOutfitLoaded += OnOutfitLoaded;
        OutfitAssetManager.OnOutfitUnloaded += OnOutfitUnloaded;
        outfitManager.AccessoryChanged += HandleAccessoryChanged;
        Settings.Favorites.OnFavoritesCleared += UnfavoriteAllAccessories;

        deselectAll = transform.Find(uncheckAllAddress).GetComponent<Button>();
        deselectAll.onClick.AddListener(outfitManager.DisableAllAccessories);
        favoriteFilter = transform.Find(filtersAddress + "/Favorites").GetComponent<Toggle>();
        favoriteFilter.onValueChanged.AddListener(ProcessFilters);
        activeFilter = transform.Find(filtersAddress + "/Active").GetComponent<Toggle>();
        activeFilter.onValueChanged.AddListener(ProcessFilters);
        showHideBase = transform.Find(showHideAddress).GetComponent<Button>();
        showHideBase.onClick.AddListener(outfitManager.ToggleBaseVisibility);

        listRoot = transform.Find(listRootAddress);

        searchBox = transform.Find(searchBoxAddress).GetComponent<InputField>();
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

        foreach (var acc in accessoryUIs.Keys) { SetAccUIVisible(acc, false); }
        foreach (var outfit in outfitUIs.Values) { outfit.gameObject.SetActive(!accFilterActive); }

        if (active) { foreach (var acc in outfitManager.ActiveAccessories) { SetAccUIVisible(acc, true); } }
        if (favorites) { foreach (var acc in Settings.Favorites.favorites) { SetAccUIVisible(acc, true); } }

        if (!textFilter) return;
        var lowerString = searchString.ToLower();
        foreach (var outfit in outfitUIs.Keys
            .Where(x => x.DisplayName
                .ToLower()
                .Contains(lowerString))
            .ToList())
        { outfitUIs[outfit].gameObject.SetActive(true); }

        foreach (var acc in accessoryUIs.Keys
            .Where(x => x.Name
                .ToLower()
                .Contains(lowerString))
            .ToList())
        { SetAccUIVisible(acc, true); }
    }

    private void OnOutfitLoaded(Outfit outfit)
    {
        var uiInstance = Instantiate(loader.OutfitListElement, listRoot);
        if (!uiInstance) { Log.Error("Failed to instantiate outfit UI prefab."); return; }

        var outfitUI = uiInstance.AddComponent<OutfitUI>();
        if (!outfitUI) { Log.Error("Failed to add OutfitUI component"); return; }
        outfitUI.Constructor(outfit, this, contextMenu);

        outfitUIs[outfit] = outfitUI;
        outfitUI.transform.SetSiblingIndex(outfitUIs.IndexOfKey(outfit));

        foreach (var accessory in outfit.Accessories)
        {
            OnAccessoryLoaded(outfitUI, accessory);
            accessoryUIs[accessory] = null;
        }
    }

    private void OnOutfitUnloaded(Outfit outfit)
    {
        if (!outfitUIs.ContainsKey(outfit)) { Log.Warning("UI instance was asked to remove a UI element that was not in it's dict"); return; }
        var outfitUI = outfitUIs[outfit];
        outfitUIs.Remove(outfit);
        Destroy(outfitUI.gameObject);
    }

    private AccessoryUI BuildAccUI(AccessoryDescriptor accessoryDescriptor)
    {
        Outfit outfit = OutfitAssetManager.GetOutfitByAssetName(accessoryDescriptor.Source);
        Log.Debug($"BuildAccUI: source: {accessoryDescriptor.Source} outfit.assetname: {outfit.AssetName}");
        StoredAccessory accessory = outfit.GetAccessory(accessoryDescriptor);
        //does this line use the outfit name comparison?
        var outfitUI = outfitUIs[outfit];

        var accInstance = Instantiate(loader.AccessoryListElement, outfitUI.transform);
        if (!accInstance) { Log.Error("Failed to instantiate accessory UI prefab."); return null; }

        var accUI = accInstance.AddComponent<AccessoryUI>();
        if (!accUI) { Log.Error("Failed to add AccUI component"); return null; }
        accUI.Constructor(outfitUI, accessory, this, contextMenu, outfitManager);
        accessoryUIs[accessoryDescriptor] = accUI;
        return accUI;
    }

    private MaterialUI BuildMatUI(AccMatSlot location, MaterialDescriptor material)
    {
        var accessoryUI = accessoryUIs[location.accessory];
        if (!accessoryUI)
        {
            accessoryUI = BuildAccUI(location.accessory);
        }
        var matInstance = Instantiate(loader.AccessoryListElement, accessoryUI.transform.parent);
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

    public void OnAccessoryLoaded(OutfitUI outfitUI, StoredAccessory accessory)
    {
        int index = 0;
        foreach (var mat in accessory.Materials) OnMaterialLoaded(accessory, mat, index++);
    }

    public void OnMaterialLoaded(StoredAccessory accessory, MaterialDescriptor material, int index)
    {
        materialUIs[new AccMatSlot { accessory = accessory, index = index }] = null;
    }

    public void SetOutfitExpanded(Outfit outfit, bool expanded)
    {
        foreach (StoredAccessory accessory in outfit.Accessories) SetAccUIVisible(accessory, expanded);
    }

    public void SetAccessoryExpanded(StoredAccessory accessory, bool expanded)
    {
        int index = 0;
        foreach (var mat in accessory.Materials) SetMatUIVisible(new AccMatSlot { accessory = accessory, index = index++ }, expanded);
    }

    private void SetMatUIVisible(AccMatSlot material, bool visible)
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

    private void SetAccUIVisible(AccessoryDescriptor accessory, bool visible)
    {
        if (!accessoryUIs.ContainsKey(accessory)) { Log.Warning($"Missing {accessory.Name}, {accessory.Source} is likely missing."); return; }
        var uiInstance = accessoryUIs[accessory];
        if (!visible) { uiInstance?.gameObject.SetActive(false); return; }

        if (!uiInstance)
        {
            uiInstance = BuildAccUI(accessory);
            if (!uiInstance) { Log.Error($"Failed to find or construct {accessory}'s UI instance during show"); return; }
        }
        if (visible) outfitUIs[OutfitAssetManager.GetOutfitByAssetName(accessory.Source)].gameObject.SetActive(true);
        uiInstance.gameObject.SetActive(true);
    }

    //TODO: there may be an issue here with some StoredAccessories not actually being unique enough on their own
    private record AccMatSlot { public StoredAccessory accessory; public int index; }

    private void HandleAccessoryChanged(AccessoryChangedEvent e)
    {
        Log.Debug("OutfitListUI.OnAccessoryStateChanged");
        if (!accessoryUIs.ContainsKey(e.Target)) { Log.Error($"Key {e.Target} was not found in dict."); return; }

        if (!accessoryUIs[e.Target]) BuildAccUI(e.Target);
        accessoryUIs[e.Target].activationToggle.SetIsOnWithoutNotify(e.Visible);

        int index = 0;
        foreach (var materialState in e.State.Materials)
        {
            var accMatSlot = new AccMatSlot { accessory = e.Target, index = index };
            if (!materialUIs[accMatSlot]) BuildMatUI(accMatSlot, e.Target.Materials[index]);
            materialUIs[accMatSlot].UpdateActiveMaterial(materialState);
            index++;
        }
    }

    private void UnfavoriteAllAccessories()
    {
        foreach (var accessory in Settings.Favorites.favorites.ToList())
        {
            accessoryUIs.TryGetValue(accessory, out var ui);
            if (!ui) { Log.Debug($"{accessory} was not in ui accessory list.");  continue; }
            Log.Debug($"Unfavoriting {accessory}");
            ui.SetFavorite(false);
        }
    }
}
