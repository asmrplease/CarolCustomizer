using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Events;
using CarolCustomizer.Models;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.UI.Main;
using CarolCustomizer.Utils;
using Onirism.Gameplay;
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

    SortedList<SourceDescriptor, OutfitUI> outfitUIs = [];
    SortedList<string, StoredHair> hairstyles = [];
    SortedList<string, HairDye> hairDyes = [];

    public Main.ContextMenu ContextMenu { get; private set; }
    public static OutfitCoordinator TargetOutfit => targetCarol?.outfitManager;
    public UIElementFactory Factory { get; private set; }
    public Transform ListRoot { get; private set; }

    public OutfitListUI Constructor(
        UIAssetLoader loader,
        Main.ContextMenu contextMenu)
    {
        Log.Debug("OutfitListUI.Constructor()");
        this.loader = loader;
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

    private void HandleHairLoaded((List<StoredHair> styles, List<HairDye> colors) hairData)
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

        hairData.styles.ForEach(x => hairstyles.TryAdd(x.DisplayName, x));
        hairData.colors.ForEach(x => hairDyes.TryAdd(LocalizationIndex.GetLine(x.localizationName), x));

        hairDyes
            .Select(x => new Dropdown.OptionData() { text = x.Key })
            .ForEach(ColorSelector.options.Add);
        hairstyles
            .Select(x => new Dropdown.OptionData() { text = x.Key})
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
        if (outfitUIs.TryGetValue(outfit.Descriptor, out var existing)) { Log.Error($"{outfit.AssetName} was already in the UI list during OnOutfitLoaded"); return; }

        var outfitUI = Factory.BuildOutfitUI(outfit);
        if (!outfitUI) { Log.Error($"failed to instantiate {outfit.DisplayName}'s outfitUI"); return; }

        outfitUIs.Add(outfit.Descriptor, outfitUI);
        outfitUI.transform.SetSiblingIndex(outfitUIs.IndexOfKey(outfit.Descriptor));
    }

    void OnOutfitUnloaded(Outfit outfit)
    {
        if (!outfitUIs.ContainsKey(outfit.Descriptor)) { Log.Warning("UI instance was asked to remove a UI element that was not in it's dict"); return; }

        var outfitUI = outfitUIs[outfit.Descriptor];
        outfitUIs.Remove(outfit.Descriptor);
        Destroy(outfitUI.gameObject);
    }

    void HandleDisableAllAccessories() => TargetOutfit.DisableAllAccessories();
    void HandleDisableAllEffects() => TargetOutfit.DisableAllEffects();

    void HandleFilterEvent(UIFilterChangedEvent eventData)
    {
        outfitUIs.Values.ForEach(x => x.HandleFilterEvent(eventData));
        if (eventData.ShowActive)
        {
            foreach(var acc in TargetOutfit.ActiveAccessories)
            {
                if (!outfitUIs.TryGetValue(acc.Source, out var ui)) continue;

                ui.SetAccUIVisible(acc);
            }
        }
        if (eventData.ShowFavorites)
        {
            Settings.Favorites
                .favorites
                .ForEach(acc => outfitUIs[acc.Source].SetAccUIVisible(acc));
        }
        Log.Info(eventData.ToString());
    }

    void HandleTargetChanged(CarolInstance newTarget)
    {
        if (newTarget is null) return;

        if (targetCarol is not null)
        {
            targetCarol.outfitManager.AccessoryChanged -= HandleAccessoryChanged;
            targetCarol.outfitManager.ActiveAccessories
                .Select(x => new AccessoryChangedEvent(x, x, false))//fire events to 'clear' the ui?
                .ForEach(HandleAccessoryChanged);
        }
        targetCarol = newTarget;
        targetCarol.outfitManager.ActiveAccessories
                .Select(x => new AccessoryChangedEvent(x, x, true))//notify the ui what the new outfit looks like
                .ForEach(HandleAccessoryChanged);
        targetCarol.outfitManager.AccessoryChanged += HandleAccessoryChanged;
        filter.ProcessFilters();
    }

    void HandleAccessoryChanged(AccessoryChangedEvent e)
    {
        //TODO: make a bulk version of this so we can update the filter faster after the recipe changes/this event
        if (e.Target.Source.Type == Models.SourceType.Hair) { HandleHairChange(e); return; }
        if (!outfitUIs.TryGetValue(e.Target.Source, out var outfitUI)) { Log.Warning($"{e.Target.Source} is not in UI foutfit list?"); return; }
        
        outfitUI.HandleAccessoryChanged(e);
    }

    void HandleHairChange(AccessoryChangedEvent e)
    {
        Log.Debug($"HandleHairChange({e})");
        var style = e.Target;
        var match = hairstyles.Values
            .FirstOrDefault(x => x.Equals(style));
        if (match is null) { Log.Error($"No hairstyle matching {e.Target}"); return; }

        int styleIndex = hairstyles.Values.IndexOf(match);
        if (styleIndex < 0) { Log.Error($"Failed to find index hairstyle {match}."); return; }

        int matIndex = hairstyles.Values[styleIndex].hairstyle.mainMaterialIndex;
        var colorDesc = e.State.Materials[matIndex];
        Log.Debug($"colorDesc: {colorDesc}");
        var localized = colorDesc.Name.DeInstance() == "CRLH_Default_Brown" ? "Carol" :
            hairDyes.FirstOrDefault(x => x.Value.material.name == colorDesc.Name).Key;
        if (localized is null) { Log.Error($"Failed to find ui index for {colorDesc.Name}"); return; }

        var colorIndex = hairDyes.IndexOfKey(localized);
        Log.Debug($"Localized:{localized}, index: {colorIndex}");

        ModelSelector.SetValueWithoutNotify(styleIndex);
        ColorSelector.SetValueWithoutNotify(colorIndex);
    }

    void UnfavoriteAllAccessories()
    {
        Settings.Favorites.favorites
            .ToList()
            .Select(acc => OutfitAssetManager.GetOutfitByAssetName(acc.Source.Name))
            .Where(outfit => outfit is not null)
            .Distinct()
            .Select(outfit => (found: outfitUIs.TryGetValue(outfit.Descriptor, out var UI), UI))
            .Where(tup => tup.found)
            .ForEach(tup => tup.UI.ClearFavorites());
    }

    void OnColorDropdownChanged(int dyeIndex)
    {
        Log.Debug($"OnColorDropdownChanged({dyeIndex})");
        var modelIndex = ModelSelector.value;
        OnHairDropdownChanged(modelIndex, dyeIndex);
        
    }

    void OnModelDropdownChanged(int modelIndex)
    {
        Log.Debug($"OnModelDropdownChanged({modelIndex})");
        var dyeIndex = ColorSelector.value;
        OnHairDropdownChanged(modelIndex, dyeIndex);
    }

    void OnHairDropdownChanged(int modelIndex, int dyeIndex)
    {
        Log.Debug($"OnHairDropdownChanged(model:{modelIndex}, dye:{dyeIndex}");
        var dye = hairDyes.Values[dyeIndex];
        var model = hairstyles.Values[modelIndex];
        var matDesc = new MaterialDescriptor(dye.material, new(Constants.HairDyeSourceName, SourceType.Hair));

        var existingHair = OutfitListUI.TargetOutfit.ActiveAccessories
            .Where(x => x.Source.Type == Models.SourceType.Hair)
            .ForEach(OutfitListUI.TargetOutfit.DisableAccessory);
        OutfitListUI.TargetOutfit.EnableAccessory(model);
        OutfitListUI.TargetOutfit.PaintAccessory(model, matDesc, model.hairstyle.mainMaterialIndex);
    }

    void OnEnable()
    {
        filter?.ProcessFilters();
    }
}
