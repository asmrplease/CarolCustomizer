using CarolCustomizer.Assets;
using CarolCustomizer.Contracts;
using CarolCustomizer.Events;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.UI.Main;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace CarolCustomizer.UI.Outfits;

internal class NewOutfitListUI : MonoBehaviour
{
    UIElementFactory factory;
    Main.ContextMenu contextMenu;
    FilterUI filter;
    ////Dictionary<SourceDescriptor, ListItem<IAccessorySource>> accSources = [];
    Dictionary<SourceDescriptor, ListItem<Outfit>> outfits = [];
    //Dictionary<SourceDescriptor, ListItem<AccessoryDescriptor>> accessories = [];
    //Dictionary<SourceDescriptor, ListItem<MaterialDescriptor>> materials = [];
    //Dictionary<SourceDescriptor, ListItem<Recipe>> recipes = [];

    public NewOutfitListUI(UIElementFactory factory, Main.ContextMenu contextMenu)
    {
        this.factory = factory;
        this.contextMenu = contextMenu;
        this.filter = this.gameObject
            .AddComponent<FilterUI>()
            .Constructor();

        OutfitAssetManager.OnOutfitLoaded += this.HandleOutfitLoaded;
        filter.FilterChanged += HandleFilterChanged;
    }

    void HandleFilterChanged(Events.UIFilterChangedEvent filterEvent)
    {
        outfits.Values.ForEach(ui => ui.OnFilterEvent(outfit => filterEvent.Filter(outfit)));
    }

    void HandleOutfitLoaded(Outfit outfit)
    {
        if (outfits.ContainsKey(outfit.Descriptor)) { Log.Error($"OutfitListUI was asked to instantiate {outfit.Descriptor}, but a matching outfit is already in the list."); return; }

        var oui = factory.BuildOutfitUI(outfit);
        if (!oui) { Log.Error("UIElementFactory failed to provide a UI element."); return; }

        var listItem = oui.gameObject
            .AddComponent<ListItem<Outfit>>()
            .Constructor(outfit, this.contextMenu);

        outfits.Add(outfit.Descriptor, listItem);
    }
}
