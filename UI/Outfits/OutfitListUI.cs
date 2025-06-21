using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Events;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.UI.Main;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Outfits;
public class OutfitListUI : MonoBehaviour
{
    #region Static Constants
    const string listRootAddress = "Scroll View/Viewport/Content";
    const string uncheckAllAddress = "Uncheck All";
    const string colorDropdownAddress = "Color Select";
    const string styleDropdownAddress = "Hair Select";
    #endregion

    UIAssetLoader loader;
    FilterUI filter;
    TargetSelectUI targetSelect;
    static CarolInstance targetCarol;
    Button deselectAll;
    Dropdown ColorSelector;
    Dropdown ModelSelector;

    SortedList<Outfit, OutfitUI> outfitUIs = new();

    public Main.ContextMenu ContextMenu { get; private set; }
    public MaterialManager MaterialManager { get; private set; }
    public static OutfitManager TargetOutfit => targetCarol?.outfitManager;
    public UIElementFactory Factory { get; private set; }
    public Transform ListRoot { get; private set; }

    public OutfitListUI Constructor(
        UIAssetLoader loader,
        MaterialManager materialManager,
        Main.ContextMenu contextMenu)
    {
        Log.Debug("OutfitListUI.Constructor()");
        this.loader = loader;
        this.MaterialManager = materialManager;
        this.ContextMenu = contextMenu;
        filter = this.gameObject
            .AddComponent<FilterUI>()
            .Constructor();
        targetSelect = this.gameObject
            .AddComponent<TargetSelectUI>();
        deselectAll = transform
            .Find(uncheckAllAddress)
            .GetComponent<Button>();
        ListRoot = transform.Find(listRootAddress);
        Factory = new(loader, this);

        OutfitAssetManager.OnOutfitLoaded += OnOutfitLoaded;
        OutfitAssetManager.OnOutfitUnloaded += OnOutfitUnloaded;
        OutfitAssetManager.OnHairLoaded += HandleHairLoaded;
        Settings.Favorites.OnFavoritesCleared += UnfavoriteAllAccessories;
        deselectAll.onClick.AddListener(HandleDisableAllAccessories);
        deselectAll.onClick.AddListener(HandleDisableAllEffects);
        filter.FilterChanged += HandleFilterEvent;
        TargetSelectUI.OnCarolSelectionChanged += HandleTargetChanged;
        HandleTargetChanged(PlayerInstances.DefaultPlayer);
        return this;
    }

    private void HandleHairLoaded((List<Hairstyle> styles, List<Material> colors) hairData)
    {
        Log.Debug("OutfitListUI.HandleHairLoaded");
        ColorSelector = transform
            .Find(colorDropdownAddress)
            .GetComponent<Dropdown>();
        ColorSelector
            .onValueChanged
            .AddListener(OnColorDropdownChanged);
        ModelSelector = transform
            .Find(styleDropdownAddress)
            .GetComponent<Dropdown>();
        ModelSelector
            .onValueChanged
            .AddListener(OnModelDropdownChanged);
        
        Log.Debug("Hair dropdown callbacks set up!");

        hairData.colors
            .Select(x => new Dropdown.OptionData() { text = x.name })
            .ForEach(ColorSelector.options.Add);
        hairData.styles
            .Select(x => new Dropdown.OptionData() { text = x.name })
            .ForEach(ModelSelector.options.Add);
        Log.Debug($"hair ui initialized, {ModelSelector.options.Count()} model options and {ColorSelector.options.Count()} color options.");
    }

    void OnDestroy()
    {
        OutfitAssetManager.OnOutfitLoaded -= OnOutfitLoaded;
        OutfitAssetManager.OnOutfitUnloaded -= OnOutfitUnloaded;
        filter.FilterChanged -= HandleFilterEvent;
    }

    void OnOutfitLoaded(Outfit outfit)
    {
        if (outfitUIs.TryGetValue(outfit, out var existing)) { Log.Error($"{outfit.AssetName} was already in the UI list during OnOutfitLoaded"); return; }

        var outfitUI = Factory.BuildOutfitUI(outfit);
        if (!outfitUI) { Log.Error($"failed to instantiate {outfit.DisplayName}'s outfitUI"); return; }

        outfitUIs.Add(outfit, outfitUI);
        outfitUI.transform.SetSiblingIndex(outfitUIs.IndexOfKey(outfit));
    }

    void OnOutfitUnloaded(Outfit outfit)
    {
        if (!outfitUIs.ContainsKey(outfit)) { Log.Warning("UI instance was asked to remove a UI element that was not in it's dict"); return; }

        var outfitUI = outfitUIs[outfit];
        outfitUIs.Remove(outfit);
        Destroy(outfitUI.gameObject);
    }

    void HandleDisableAllAccessories() => TargetOutfit.DisableAllAccessories();
    void HandleDisableAllEffects() => TargetOutfit.DisableAllEffects();

    void HandleFilterEvent(UIFilterChangedEvent eventData)
    {
        outfitUIs.Values.ForEach(x => x.HandleFilterEvent(eventData));
        if (eventData.ShowActive)
        {
            TargetOutfit.ActiveAccessories
                .ForEach(acc => outfitUIs[acc.outfit].SetAccessoryVisible(acc));
        }
        if (eventData.ShowFavorites)
        {
            Settings.Favorites
                .favorites
                .Select(acc =>  
                    (outfit: OutfitAssetManager.GetOutfitByAssetName(acc.Source)
                    ,acc))
                .Where(tup => tup.outfit is not null)
                .ForEach(tup => outfitUIs[tup.outfit].SetAccessoryVisible(tup.acc));
        }
        Log.Info(eventData.ToString());
    }

    void HandleTargetChanged(CarolInstance newTarget)
    {
        if (newTarget is null) return;

        if (targetCarol is not null)
        {
            targetCarol.outfitManager.AccessoryChanged -= HandleAccessoryChanged;
            targetCarol.outfitManager.HairstyleChanged -= HandleHairChange;
            targetCarol.outfitManager.ActiveAccessories
                .Select(x => new AccessoryChangedEvent(x, x, false))//fire events to 'clear' the ui?
                .ForEach(HandleAccessoryChanged);
        }
        targetCarol = newTarget;
        targetCarol.outfitManager.ActiveAccessories
                .Select(x => new AccessoryChangedEvent(x, x, true))//notify the ui what the new outfit looks like
                .ForEach(HandleAccessoryChanged);
        targetCarol.outfitManager.AccessoryChanged += HandleAccessoryChanged;
        targetCarol.outfitManager.HairstyleChanged += HandleHairChange;
        filter.ProcessFilters();
    }

    void HandleAccessoryChanged(AccessoryChangedEvent e)
    {
        //TODO: make a bulk version of this so we can update the filter after the recipe changes/this event
        if (!outfitUIs.TryGetValue(e.Target.outfit, out var outfitUI)) { Log.Warning($"{e.Target.outfit.DisplayName} is not in outfit list?"); return; }
        
        outfitUI.HandleAccessoryChanged(e);
    }

    void HandleHairChange(HairChangeEvent e)
    {
        string style = e.Style;
        string color = e.Color;

        var styleIndex = ModelSelector
            .options
            .Select(x => x.text)
            .ToList()
            .IndexOf(style);
        var colorIndex = ColorSelector
            .options
            .Select(x => x.text)
            .ToList()
            .IndexOf(color);

        ModelSelector.SetValueWithoutNotify(styleIndex);
        ColorSelector.SetValueWithoutNotify(colorIndex);
    }

    void UnfavoriteAllAccessories()
    {
        Settings.Favorites.favorites
            .ToList()
            .Select(acc => OutfitAssetManager.GetOutfitByAssetName(acc.Source))
            .Where(outfit => outfit is not null)
            .Distinct()
            .Select(outfit => (found: outfitUIs.TryGetValue(outfit, out var UI), UI))
            .Where(tup => tup.found)
            .ForEach(tup => tup.UI.ClearFavorites());
    }

    void OnColorDropdownChanged(int index)
    {
        var mat = OutfitAssetManager.HairColors[index];
        OutfitListUI.TargetOutfit.SetHairColor(mat);
    }

    void OnModelDropdownChanged(int index)
    {
        Log.Debug($"OnHairModelChanged[{index}]");
        var style = OutfitAssetManager.Hairstyles[index];
        OutfitListUI.TargetOutfit.SetHairstyle(style);
    }
}
