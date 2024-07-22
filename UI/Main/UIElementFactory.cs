using CarolCustomizer.Assets;
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
        Log.Debug("UIElementFactory.ctor");
        this.assetLoader = assetLoader; 
        this.uiInstance = uiInstance;
    }

    public OutfitUI BuildOutfitUI(Outfit outfit)
    {
        Log.Debug($"OutfitListUI.OnOutfitLoaded({outfit.AssetName})");
        var outfitUIGO = GameObject.Instantiate(assetLoader.OutfitListElement, uiInstance.ListRoot);
        if (!outfitUIGO) { Log.Error("Failed to instantiate outfit UI prefab."); return null; }
        //else Log.Debug("outfitUIGO instantiated");

        var outfitUI = outfitUIGO.AddComponent<OutfitUI>();
        if (!outfitUI) { Log.Error("Failed to add OutfitUI component"); return null; }
        //else Log.Debug("OutfitUI component created");

        outfitUI.Constructor(outfit, uiInstance);
        //Log.Debug("BuildOutfitUI complete");
        return outfitUI;
    }

    public AccessoryUI BuildAccUI(OutfitUI parent, AccessoryDescriptor accessoryDescriptor)
    {
        Outfit outfit = parent.outfit;//OutfitAssetManager.GetOutfitByAssetName(accessoryDescriptor.Source);
        //Log.Debug($"BuildAccUI: source: {accessoryDescriptor.Source} outfit.assetname: {outfit.AssetName}");
        StoredAccessory accessory = outfit.GetAccessory(accessoryDescriptor);
        //var parent = outfitUIs[outfit];

        var accInstance = GameObject.Instantiate(assetLoader.AccessoryListElement, parent.transform);
        if (!accInstance) { Log.Error("Failed to instantiate accessory UI prefab."); return null; }

        var accUI = accInstance.AddComponent<AccessoryUI>();
        if (!accUI) { Log.Error("Failed to add AccUI component"); return null; }
        accUI.Constructor(parent, accessory, uiInstance);
        //accessoryUIs[accessoryDescriptor] = accUI;
        //Log.Debug("Done"); 
        return accUI;
    }

    public MaterialUI BuildMatUI(AccessoryUI parent, AccMatSlot location, MaterialDescriptor material)
    {
        //var parent = accessoryUIs[location.accessory];
        //if (!parent) { parent = BuildAccUI(location.accessory); }

        var matInstance = GameObject.Instantiate(assetLoader.AccessoryListElement, parent.transform.parent);
        if (!matInstance) { Log.Error("Failed to instantiate material UI prefab."); return null; }

        int index = parent.transform.GetSiblingIndex() + location.index + 1;
        matInstance.transform.SetSiblingIndex(index);

        var matUI = matInstance.AddComponent<MaterialUI>();
        if (!matUI) { Log.Error("Failed to add MatUI component"); return null; }

        matUI.Constructor(parent, material, location.index, uiInstance);
        //parent.AddMaterial(matUI);
        //materialUIs[location] = matUI;
        return matUI;
    }
}
