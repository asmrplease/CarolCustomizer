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
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Outfits;
public class OutfitListUI : MonoBehaviour
{
    #region Static Constants
    const string listRootAddress = "Scroll View/Viewport/Content";
    const string scrollViewAddress = "Scroll View";
    const string uncheckAllAddress = "Uncheck All";
    const string targetSelectAddress = "Target Select";
    const int TargetSelectHeight = 32;
    #endregion

    UIAssetLoader loader;
    FilterUI filter;
    CarolInstance targetCarol;
    Button deselectAll;
    Dropdown TargetSelect;
    RectTransform scrollView;

    SortedList<Outfit, OutfitUI> outfitUIs = new();

    public Main.ContextMenu ContextMenu { get; private set; }
    public MaterialManager MaterialManager { get; private set; }
    public OutfitManager TargetOutfit => targetCarol?.outfitManager;
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
        TargetSelect = transform
            .Find(targetSelectAddress)
            .GetComponent<Dropdown>();
        deselectAll = transform
            .Find(uncheckAllAddress)
            .GetComponent<Button>();
        ListRoot = transform.Find(listRootAddress);
        Factory = new(loader, this);
        scrollView = transform.Find(scrollViewAddress) as RectTransform;

        OutfitAssetManager.OnOutfitLoaded += OnOutfitLoaded;
        OutfitAssetManager.OnOutfitUnloaded += OnOutfitUnloaded;
        Settings.Favorites.OnFavoritesCleared += UnfavoriteAllAccessories;
        MenuToggle.OnMenuToggle += HandleMenuToggle;
        deselectAll.onClick.AddListener(HandleDisableAllAccessories);
        deselectAll.onClick.AddListener(HandleDisableAllEffects);
        filter.FilterChanged += HandleFilterEvent;
        HandleTargetChanged(PlayerInstances.DefaultPlayer);
        
        return this;
    }

    void OnDestroy()
    {
        OutfitAssetManager.OnOutfitLoaded -= OnOutfitLoaded;
        OutfitAssetManager.OnOutfitUnloaded -= OnOutfitUnloaded;
        filter.FilterChanged -= HandleFilterEvent;
        MenuToggle.OnMenuToggle -= HandleMenuToggle;
    }

    void HandleMenuToggle(bool state)
    {
        Log.Debug("OutfitListUI.HandleMenuToggle()");
        Log.Debug(PlayerInstances.ValidPlayers.Count().ToString());
        if (PlayerInstances.ValidPlayers.Count() > 1)
        {
            ShowTargetSelect();
            return;
        }
        HideTargetSelect(); 
    }

    void ShowTargetSelect()
    {
        if (TargetSelect.gameObject.activeSelf) return;

        //when the number of players changes, we need to change the list options
        TargetSelect.gameObject.SetActive(true);
        var size = scrollView.sizeDelta;
        size.y -= TargetSelectHeight;
        scrollView.sizeDelta = size;
    }

    void HideTargetSelect()
    {
        if (!TargetSelect.gameObject.activeSelf) return;

        TargetSelect.gameObject.SetActive(false);
        var size = scrollView.sizeDelta;
        size.y += TargetSelectHeight;
        scrollView.sizeDelta = size;
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

    void HandleDisableAllAccessories() => this.TargetOutfit.DisableAllAccessories();
    void HandleDisableAllEffects() => this.TargetOutfit.DisableAllEffects();

    void HandleFilterEvent(UIFilterChangedEvent eventData)
    {
        outfitUIs.Values.ForEach(x => x.HandleFilterEvent(eventData));
    }

    void HandleTargetChanged(CarolInstance target)
    {
        if (target is null) return;

        if (targetCarol is not null)
        {
            targetCarol.outfitManager.AccessoryChanged -= HandleAccessoryChanged;
            targetCarol.outfitManager.ActiveAccessories
                .Select(x => new AccessoryChangedEvent(x, x, false))//fire events to 'clear' the ui?
                .ForEach(HandleAccessoryChanged);
        }
        targetCarol = target;
        targetCarol.outfitManager.ActiveAccessories
                .Select(x => new AccessoryChangedEvent(x, x, true))//notify the ui what the new outfit looks like
                .ForEach(HandleAccessoryChanged);
        targetCarol.outfitManager.AccessoryChanged += HandleAccessoryChanged;
    }

    void HandleAccessoryChanged(AccessoryChangedEvent e)
    {
        Log.Debug("OutfitListUI.HandleAccessoryChanged()");
        if (!outfitUIs.TryGetValue(e.Target.outfit, out var outfitUI)) { Log.Warning($"{e.Target.outfit.DisplayName} is not in outfit list?"); return; }
        
        outfitUI.HandleAccessoryChanged(e);
    }

    void UnfavoriteAllAccessories()
    {
        //Settings.Favorites.favorites
        //    .ToList()
        //    .Select(acc =>
        //        (found: accessoryUIs.TryGetValue(acc, out var ui)
        //        ,ui: ui))
        //    .Where(tup => tup.found)
        //    .ForEach(tup => tup.ui.SetFavorite(false));
    }
}
