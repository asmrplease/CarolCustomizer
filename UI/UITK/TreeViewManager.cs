using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Contracts;
using CarolCustomizer.UI.UITK;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using TreeView = UnityEngine.UIElements.TreeView;

public class TreeViewManager : MonoBehaviour
{

    [SerializeField]
    VisualTreeAsset elementTemplate;

    [SerializeField]
    Dictionary<int, IListable> elements = new();

    UITKAssetLoader assetLoader;
    TreeView treeView;

    public TreeViewManager Constructor(UITKAssetLoader assetLoader)
    {
        this.assetLoader = assetLoader;
        this.elementTemplate = assetLoader.ListItem;

        var uiDoc = this.gameObject.AddComponent<UIDocument>();
        uiDoc.visualTreeAsset = assetLoader.TreeView;
        uiDoc.sourceAsset = assetLoader.TreeView;
        uiDoc.panelSettings = assetLoader.PanelSettings;
        var root = uiDoc.rootVisualElement;
        treeView = root.Q<TreeView>();
        treeView.fixedItemHeight = 100;

        OutfitAssetManager.OnOutfitLoaded += AddListable;
        RecipeFileWatcher.OnRecipeCreated += AddListable;

        return this;
    }

    void Start()
    {
        //instantiate/reference the element template
        if (this.elementTemplate is null) return;

        //set up closures for element instantiation and binding
        treeView.makeItem = () => elementTemplate.CloneTree();
        treeView.bindItem = (element, index) =>
        {
            var listable = GetListableAtIndex(index);
            if (listable is null) return;

            element.Q<Label>("Header").text = listable.Header;
            element.Q<Label>("Subheader").text = listable.Subheader;
            element.Q("Thumbnail").style.backgroundImage = new StyleBackground(listable.Thumbnail);
            element.RegisterCallback<PointerDownEvent, IListable>(Click, listable);
        };
        var initializer = new List<TreeViewItemData<IListable>>();
        treeView.SetRootItems(initializer);
    }

    void Click(PointerDownEvent e, IListable element)
    {
        Log.Debug(element.Header);
        if (e.button != (int)Settings.HotKeys.ContextMenu) return;
        if (element is not IContextMenuActions clickable) return;

        //clickable.Test();
        UITKContextMenu.Show(clickable);
    }

    public void AddListable(IListable newListable)
    {
        var element = newListable.ToTVID();
        FlattenTree(element);
        this.treeView.AddItem(element);
    }

    void FlattenTree(TreeViewItemData<IListable> tvid)
    {
        elements[tvid.id] = tvid.data;
        tvid.children.ForEach(FlattenTree);
    }

    IListable GetListableAtIndex(int index)
    {
        //get the id of the element at this index
        //if (this.treeView.viewController is null) return null;
        var id = this.treeView.viewController.GetIdForIndex(index);
        //get the element that has that id
        if (!elements.TryGetValue(id, out var result)) Debug.LogWarning("failed to find element");
        return result;
        //return this.elements[index];
    }
}
