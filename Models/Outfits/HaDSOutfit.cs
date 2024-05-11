using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static System.Linq.Enumerable;

namespace CarolCustomizer.Models.Outfits;
public class HaDSOutfit : Outfit
{
    #region ModelData Handling 
    public override Sprite Sprite => modelData.portraitShop;
    public override RuntimeAnimatorController RuntimeAnimator => modelData?.controller;
    public ModelData modelData { get; protected set; }

    public Dictionary<string, List<StoredAccessory>> Variants { get; private set; } = new();

    public HaDSOutfit(Transform storedAsset) : base(storedAsset)
    {
        modelData = this.storedAsset.gameObject.GetComponent<ModelData>();
        if (!modelData) { Log.Error("No ModelData component was found during HaDS constructor."); return; }
        BuildVariants();
    }

    protected virtual void BuildVariants()
    {
        Log.Debug("HaDS Variants");
        foreach (int i in Range(0, modelData.accessories.Count))
        {
            if (compData.coopToggles.Count() == 0)
            {
                Variants[modelData.accessories[i].name] = BuildVariant(i, -1);
                continue;
            }
            foreach (int j in Range(0, compData.coopMeshes.Count()))
            {
                string variantName = $"P({j + 1}) {modelData.accessories[i].name}";
                Variants[variantName] = BuildVariant(i, j);
            }
        }
    }

    List<StoredAccessory> BuildVariant(int accessoryGroup, int coopToggle)
    {
        var results = Accessories.ToDictionary(x=>x, y=>true);

        foreach (int i in Range(0, modelData.accessories.Count))
        {
            foreach (var acc in modelData.accessories[i].objects
                .Select(x=> x? GetAccessory(x.name) : null))
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
                .Select(x => GetAccessory(new AccessoryDescriptor(x, AssetName))))
            {
                if (!results[acc]) continue;//if it's already been disabled, don't turn it back on
                results[acc] = (j == coopToggle);
            }
        }
        return results.Where(x=>x.Value).Select(x=>x.Key).ToList();
    }
    #endregion
}