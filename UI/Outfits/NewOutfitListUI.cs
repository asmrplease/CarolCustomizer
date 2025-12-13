using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.UI.Main;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.UI.Outfits;

internal class NewOutfitListUI : MonoBehaviour
{
    const string listRootAddress = "Scroll View/Viewport/Content";
    UIElementFactory factory;
    Main.ContextMenu contextMenu;
    RecipeFileWatcher recipeWatcher;
    FilterUI filter;
    Transform listRoot;
    Dictionary<PathDescriptor, ListItem> directories = [];
    Dictionary<IListable, ListItem> listList = [];

    public NewOutfitListUI Constructor(UIElementFactory factory, RecipeFileWatcher recipeWatcher, Main.ContextMenu contextMenu)
    {
        this.factory = factory;
        this.contextMenu = contextMenu;
        this.recipeWatcher = recipeWatcher;
        listRoot = transform.Find(listRootAddress);
        this.filter = this.gameObject
            .AddComponent<FilterUI>()
            .Constructor();

        OutfitAssetManager.OnOutfitLoaded += this.HandleListable;
        OutfitAssetManager.OnHairLoaded += HandleHairLoaded;
        recipeWatcher.OnRecipeCreated += HandleListable;
        filter.FilterChanged += HandleFilterChanged;

        return this;
    }

    void HandleHairLoaded((List<Models.Accessories.StoredHair>, List<Onirism.Gameplay.HairDye>) obj)
    {
        obj.Item1.ForEach(HandleListable);
        if (OutfitAssetManager.HairDyes is not HairDyeSource dyeSource) { return; }
        
        HandleListable(dyeSource);
    }

    void HandleFilterChanged(Events.UIFilterChangedEvent filterEvent)
    {
        //outfits.Values.ForEach(ui => ui.OnFilterEvent(outfit => filterEvent.Filter(outfit)));
        //outfits.Values.ForEach(ui => ui.OnFilterEvent(filterEvent));
    }

    void HandleListable(IListable listable)
    {
        if (listList.TryGetValue(listable, out var existing)) { Log.Warning($"IListable {listable.Header} has already been instantiated"); return; }

        var element = factory.BuildGeneric(listable, this.listRoot, this.contextMenu);
        if (!element) { Log.Error("UIElementFactory failed to provide a UI element."); return; }

        listList[listable] = element;
        if (listable is not IPath path) return;

        var parent = HandleDirectorySetup(path.PathDescriptor);
        element.ParentTo(parent);
        element.gameObject.SetActive(false);
        return;
    }

    ListItem HandleDirectorySetup(PathDescriptor path)
    {
        Log.Debug($"DirectorySetup({path.Path})");
        if (directories.TryGetValue(path, out var existing)) { return existing; }
        //create listable for the directory
        var directory = new DirectoryListing(path);
        var listItem = factory.BuildGeneric(directory, this.listRoot, this.contextMenu);
        directories[path] = listItem;
        return listItem;
    }
}
