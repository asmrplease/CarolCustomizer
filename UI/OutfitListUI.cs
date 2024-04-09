using CarolCustomizer.Assets;
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
    
    //do we need to break down this class?
    //how do we break down this class?
        //UI element instantiation
        //filter processing
        //ui event handling
        //clothing event handling

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

        foreach (var acc in accessoryUIs.Keys)
        {
            if (favorites && favoritesManager.IsInFavorites(acc)) { SetAccUIVisible(acc, true); continue; }
            if (active && outfitManager.IsEnabled(acc)) { SetAccUIVisible(acc, true); continue; }

            if (searchString == "") { SetAccUIVisible(acc, false); continue; }
            bool textSearchResult = acc.DisplayName.ToLower().Contains(searchString.ToLower());
            if (textSearchResult) { SetAccUIVisible(acc, true); continue; }

            SetAccUIVisible(acc, false);
        }
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
            OnAccessoryLoaded(outfitUI, accessory);
            accessoryUIs.Add(accessory, null);
        }
    }

    private void OnOutfitUnloaded(Outfit outfit)
    {
        if (!outfitUIs.ContainsKey(outfit)) { Log.Warning("UI instance was asked to remove a UI element that was not in it's dict"); return; }
        var outfitUI = outfitUIs[outfit];
        outfitUIs.Remove(outfit);
        GameObject.Destroy(outfitUI.gameObject);
    }

    private AccessoryUI BuildAccUI(StoredAccessory accessory)
    {
        var outfitUI = this.outfitUIs[accessory.outfit];

        var accInstance = GameObject.Instantiate(loader.AccessoryListElement, outfitUI.transform);
        if (!accInstance) { Log.Error("Failed to instantiate accessory UI prefab."); return null; }

        var accUI = accInstance.AddComponent<AccessoryUI>();
        if (!accUI) { Log.Error("Failed to add AccUI component"); return null; }
        accUI.Constructor(outfitUI, accessory, this, contextMenu, outfitManager, favoritesManager);
        this.accessoryUIs[accessory] = accUI;
        return accUI;
    }

    private MaterialUI BuildMatUI(AccMatSlot location, MaterialDescriptor material) 
    {
        var accessoryUI = this.accessoryUIs[location.accessory];
        if (!accessoryUI) 
        {
            accessoryUI = BuildAccUI(location.accessory);
        }
        var matInstance = GameObject.Instantiate(this.loader.AccessoryListElement, accessoryUI.transform.parent);
        if (!matInstance) { Log.Error("Failed to instantiate material UI prefab."); return null; }

        int index = accessoryUI.transform.GetSiblingIndex() + location.index + 1;
        matInstance.transform.SetSiblingIndex(index);

        var matUI = matInstance.AddComponent<MaterialUI>();
        if (!matUI) { Log.Error("Failed to add MatUI component"); return null; }

        matUI.Constructor(accessoryUI, material, location.index, this, contextMenu, outfitManager);
        accessoryUI.AddMaterial(matUI);
        this.materialUIs[location] = matUI;
        return matUI;
    }

    public void OnAccessoryLoaded(OutfitUI outfitUI, StoredAccessory accessory)
    {
        int index = 0;
        foreach (var mat in accessory.Materials) OnMaterialLoaded(accessory, mat, index++);
    }

    public void OnMaterialLoaded(StoredAccessory accessory, MaterialDescriptor material, int index)
    {
        materialUIs.Add(new AccMatSlot { accessory = accessory, index = index }, null);
    }

    public void SetOutfitExpanded(Outfit outfit, bool expanded)
    {
        foreach (StoredAccessory accessory in outfit.Accessories) SetAccUIVisible(accessory, expanded);
    }

    public void SetAccessoryExpanded(StoredAccessory accessory, bool expanded)
    {
        int index = 0;
        foreach (var mat in accessory.Materials) SetMatUIVisible(new AccMatSlot { accessory = accessory, index = index }, expanded);
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

    private void SetAccUIVisible(StoredAccessory accessory, bool visible)
    {
        var uiInstance = accessoryUIs[accessory];
        if (!visible) { uiInstance?.gameObject.SetActive(false); return; }

        if (!uiInstance)
        {
            uiInstance = BuildAccUI(accessory);
            if (!uiInstance) { Log.Error($"Failed to find or construct {accessory}'s UI instance during show"); return; }
        }
        if (visible) outfitUIs[accessory.outfit].gameObject.SetActive(true);
        uiInstance.gameObject.SetActive(true);
    }

    internal void SetBaseOutfit(Outfit outfit) => outfitManager.SetBaseOutfit(outfit);

    private void OnDisableAll() => outfitManager.DisableAllAccessories();

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
            var accMatSlot = new AccMatSlot { accessory = e.Target, index = index};
            if (!materialUIs[accMatSlot]) BuildMatUI(accMatSlot, e.Target.Materials[index]);
            materialUIs[accMatSlot].UpdateActiveMaterial(materialState);
            index++;
        }
    }
}
