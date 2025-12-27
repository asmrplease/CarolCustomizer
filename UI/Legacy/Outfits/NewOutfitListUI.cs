using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Contracts;
using CarolCustomizer.Events;
using CarolCustomizer.Models;
using CarolCustomizer.UI.Legacy.Main;
using CarolCustomizer.UI.Main;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.UI.Legacy.Outfits;

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
    List<ListItem> topList = [];

    public NewOutfitListUI Constructor(
        UIElementFactory factory, 
        RecipeFileWatcher recipeWatcher,
        Main.ContextMenu contextMenu)
    {
        this.factory = factory;
        this.contextMenu = contextMenu;
        this.recipeWatcher = recipeWatcher;
        listRoot = transform.Find(listRootAddress);
        filter = gameObject
            .AddComponent<FilterUI>()
            .Constructor();

        OutfitAssetManager.OnOutfitLoaded += HandleTopList;
        OutfitAssetManager.OnHairLoaded += HandleHairLoaded;
        RecipeFileWatcher.OnRecipeCreated += HandleTopList;
        filter.FilterChanged += HandleFilterChanged;
        TargetSelectUI.OnCarolSelectionChanged += HandleTargetChanged; ;
        HandleTargetChanged(PlayerInstances.DefaultPlayer);

        return this;
    }

    private void HandleTargetChanged(Behaviors.Carol.CarolInstance obj)
    {
        throw new System.NotImplementedException();
    }

    void HandleHairLoaded((List<Models.Accessories.StoredHair>, List<Onirism.Gameplay.HairDye>) obj)
    {
        obj.Item1.ForEach(HandleTopList);
        HandleTopList(OutfitAssetManager.HairDyes);
    }

    void HandleFilterChanged(UIFilterChangedEvent e)
    {
        Log.Debug($"NewOutfitListUI.HandleFilterChanged({e.AnyFilters})");
        if (e.AnyFilters)
        {
            listList.Values
            .Select(x => (ui: x, filter: x as IFilterable<UIFilterChangedEvent>))
            .ForEach(tup => tup.ui.gameObject.SetActive(tup.filter.MatchesFilter(e)));
        }
        else
        {
            listList.Values.ForEach(x => x.gameObject.SetActive(false));
            topList.ForEach(x => x.gameObject.SetActive(true));
        }
    }

    void HandleTopList(IListable listable) => HandleSubList(listable, true);

    ListItem HandleSubList(IListable listable, bool top = false)
    {
        if (listList.TryGetValue(listable, out var existing)) { Log.Warning($"IListable {listable.Header} has already been instantiated"); return null; }

        var element = factory.BuildGeneric(listable, listRoot, contextMenu, top);
        if (!element) { Log.Error("UIElementFactory failed to provide a UI element."); return null; }

        listList[listable] = element;
        listable.Children
            .Select(x => HandleSubList(x))
            .Where(x => x is not null)
            .ForEach(x => ListItem.Parent(element, x));
        if (listable is not IPath path)
        {
            if (top) topList.Add(element);
            return element;
        }

        var parent = HandleDirectorySetup(path);
        ListItem.Parent(parent, element);
        element.gameObject.SetActive(false);
        return element;
    }

    ListItem HandleDirectorySetup(IPath path) 
    {
        Log.Debug($"DirectorySetup({path.PathDescriptor.Path})");
        if (directories.TryGetValue(path.PathDescriptor, out var existing)) { return existing; }

        var directory = new DirectoryListing(path.PathDescriptor);
        var listItem = factory.BuildGeneric(directory, listRoot, contextMenu, true);
        topList.Add(listItem);
        directories[path.PathDescriptor] = listItem;
        listList[directory] = listItem;
        return listItem;
    }
}
