using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static System.Linq.Enumerable;

namespace CarolCustomizer.Models.Outfits;
public partial class HaDSOutfit : Outfit
{
    #region ModelData Handling 
    //public override Sprite Thumbnail => modelData.portraitShop;
    public override RuntimeAnimatorController GetAnimator() => modelData?.controller;
    public override ModelData GetConfiguration() => modelData;
    public ModelData modelData { get; protected set; }
    public HairData hairData { get; protected set; }

    public HaDSOutfit(Transform storedAsset) : base(storedAsset)
    {
        Log.Debug($"Loading HaDSOutfit {storedAsset.name}");
        modelData = this.storedAsset.gameObject.GetComponent<ModelData>();
        if (!modelData) { Log.Error("No ModelData component was found during HaDS constructor."); return; }
        this.Thumbnail = modelData.portraitShop;
        BuildVariants();
        GetHair();
    }

    void GetHair()
    {
        var hair = this.modelData.defaultHairstyle;
        if (!hair) return;

        this.hairData = new HairData(hair.transform);
        this.hairData.models.ForEach(x => x.gameObject.SetActive(false));//disables built in hair when it's instantiated on carol by game
    }

    protected virtual void BuildVariants()
    {
        foreach (int i in Range(0, modelData.accessories.Count))
        {
            if (compData.coopToggles.Count() == 0)
            {
                string idk = modelData.accessories[i].name;
                this.Variants[idk] = new Recipe(idk, BuildRecipe(i, -1), this.Thumbnail);
                continue;
            }
            foreach (int j in Range(0, compData.coopMeshes.Count()))
            {
                string variantName = $"P({j + 1}) {modelData.accessories[i].name}";
                this.Variants[variantName] = new Recipe(variantName, BuildRecipe(i, j), this.Thumbnail);
            }
        }
    }

    RecipeDescriptor BuildRecipe(int accessoryGroup, int coopToggle)
    {
        var accessories = BuildVariant(accessoryGroup, coopToggle)
            .Select(x => x as AccessoryDescriptor)
            .ToList();
        if (modelData && modelData.defaultHairstyle && modelData.defaultHaircolor)
        {
            var hairstyle = RecipeConverter.GetHairFromStrings(modelData.defaultHairstyle.name, modelData.defaultHaircolor.name);
            accessories.Add(hairstyle);
        }

        return BuildRecipe(accessories);
    }

    protected RecipeDescriptor BuildRecipe(IEnumerable<AccessoryDescriptor> accessories)
    {
        return new RecipeDescriptor25
        (
            this.Descriptor,
            this.Descriptor,
            this.Descriptor,
            accessories,
            [this.Descriptor],
            PluginInfo.PLUGIN_VERSION
        );
    }

    List<StoredAccessory> BuildVariant(int accessoryGroup, int coopToggle)
    {
        var results = GetAccessories().ToDictionary(x => x, y => true);

        foreach (int i in Range(0, modelData.accessories.Count))
        {
            foreach (var acc in modelData.accessories[i].objects
                .Select(x => x? GetAccessoryByName(x.name) : null))
            {
                if (acc is null) continue;
                if (!results[acc]) continue;//if it's already been disabled, don't turn it back on
                results[acc] = (i == accessoryGroup);
            }
        }
        if (coopToggle == -1) { return results.Where(x => x.Value).Select(x => x.Key).ToList(); }

        foreach (int j in Range(0, compData.coopMeshes.Count()))
        {
            foreach (var acc in compData.coopMeshes[j]
                .Select(x => GetAccessory(new AccessoryDescriptor(x, new SourceDescriptor(AssetName, SourceType.Outfit)))))
            {
                if (!results[acc]) continue;//if it's already been disabled, don't turn it back on
                results[acc] = (j == coopToggle);
            }
        }
        return results.Where(x=>x.Value).Select(x=>x.Key).ToList();
    }
    #endregion
}

public partial class HaDSOutfit : IPath
{
    PathDescriptor IPath.PathDescriptor => new("Outfits", PathType.Convenience);
}