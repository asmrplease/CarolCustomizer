using CarolCustomizer.Assets;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.UI.Main;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace CarolCustomizer.UI.Outfits;

internal class NewOutfitListUI : MonoBehaviour
{
    const string listRootAddress = "Scroll View/Viewport/Content";
    UIElementFactory factory;
    Main.ContextMenu contextMenu;
    FilterUI filter;
    Transform listRoot;
    ////Dictionary<SourceDescriptor, ListItem<IAccessorySource>> accSources = [];
    Dictionary<SourceDescriptor, ListItem> outfits = [];
    //Dictionary<SourceDescriptor, ListItem<AccessoryDescriptor>> accessories = [];
    //Dictionary<SourceDescriptor, ListItem<MaterialDescriptor>> materials = [];
    //Dictionary<SourceDescriptor, ListItem<Recipe>> recipes = [];

    public NewOutfitListUI Constructor(UIElementFactory factory, Main.ContextMenu contextMenu)
    {
        this.factory = factory;
        this.contextMenu = contextMenu;
        listRoot = transform.Find(listRootAddress);
        this.filter = this.gameObject
            .AddComponent<FilterUI>()
            .Constructor();

        OutfitAssetManager.OnOutfitLoaded += this.HandleOutfitLoaded;
        filter.FilterChanged += HandleFilterChanged;

        return this;
    }

    void HandleFilterChanged(Events.UIFilterChangedEvent filterEvent)
    {
        //outfits.Values.ForEach(ui => ui.OnFilterEvent(outfit => filterEvent.Filter(outfit)));
        outfits.Values.ForEach(ui => ui.OnFilterEvent(filterEvent));
    }

    void HandleOutfitLoaded(Outfit outfit)
    {
        if (outfits.ContainsKey(outfit.Descriptor)) { Log.Error($"OutfitListUI was asked to instantiate {outfit.Descriptor}, but a matching outfit is already in the list."); return; }

        var element = factory.BuildGeneric(outfit, this.listRoot, this.contextMenu);
        if (!element) { Log.Error("UIElementFactory failed to provide a UI element."); return; }

        outfits.Add(outfit.Descriptor, element);
    }
}
