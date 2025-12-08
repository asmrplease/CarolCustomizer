using CarolCustomizer.Assets;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.UI.Outfits;
using CarolCustomizer.Utils;
using UnityEngine;

namespace CarolCustomizer.UI.Main;
public class UIElementFactory
{
    UIAssetLoader assetLoader;
    OutfitListUI uiInstance;
    public UIElementFactory(UIAssetLoader assetLoader, OutfitListUI uiInstance)
    {
        this.assetLoader = assetLoader; 
        this.uiInstance = uiInstance;
    }

    public UIElementFactory(UIAssetLoader assetLoader)
    {
        this.assetLoader = assetLoader;
        this.uiInstance = null;
    }

    internal ListItem BuildGeneric(IListable listable, Transform parent, ContextMenu menu)
    {
        var gameObject = GameObject.Instantiate(assetLoader.OutfitListElement, parent);
        if (!gameObject) { Log.Error("Failed to instantiate outfit UI prefab."); return null; }

        //listable.Children.ForEach(x => BuildGeneric(x, gameObject.transform, menu));

        return gameObject
            .AddComponent<ListItem>()
            .Constructor(listable, menu);
    }

    public OutfitUI BuildOutfitUI(Outfit outfit)
    {
        var outfitUIGO = GameObject.Instantiate(assetLoader.OutfitListElement, uiInstance.ListRoot);
        if (!outfitUIGO) { Log.Error("Failed to instantiate outfit UI prefab."); return null; }

        var outfitUI = outfitUIGO.AddComponent<OutfitUI>();
        if (!outfitUI) { Log.Error("Failed to add OutfitUI component"); return null; }

        outfitUI.Constructor(outfit, uiInstance);
        return outfitUI;
    }

    public AccessoryUI BuildAccUI(OutfitUI parent, AccessoryDescriptor accessoryDescriptor)
    {
        Outfit outfit = parent.outfit;
        StoredAccessory accessory = outfit.GetAccessory(accessoryDescriptor);
        var accInstance = GameObject.Instantiate(assetLoader.AccessoryListElement, parent.transform);
        if (!accInstance) { Log.Error("Failed to instantiate accessory UI prefab."); return null; }

        var accUI = accInstance.AddComponent<AccessoryUI>();
        if (!accUI) { Log.Error("Failed to add AccUI component"); return null; }

        accUI.Constructor(parent, accessory, uiInstance);
        return accUI;
    }

    public MaterialUI BuildMatUI(AccessoryUI parent, int matIndex, MaterialDescriptor material)
    {
        var matInstance = GameObject.Instantiate(assetLoader.AccessoryListElement, parent.transform.parent);
        if (!matInstance) { Log.Error("Failed to instantiate material UI prefab."); return null; }

        var matUI = matInstance.AddComponent<MaterialUI>();
        if (!matUI) { Log.Error("Failed to add MatUI component"); return null; }

        int siblingIndex = parent.transform.GetSiblingIndex() + matIndex + 1;
        matInstance.transform.SetSiblingIndex(siblingIndex);
        matUI.Constructor(parent, material, matIndex, uiInstance);
        return matUI;
    }
}
