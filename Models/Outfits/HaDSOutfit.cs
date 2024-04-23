using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using static System.Linq.Enumerable;

namespace CarolCustomizer.Models.Outfits;
public class HaDSOutfit : Outfit
{
    #region ModelData Handling 
    public override Sprite Sprite => modelData.portraitShop;

    public ModelData modelData { get; protected set; }

    public Dictionary<string, List<StoredAccessory>> Variants { get; private set; } = new();

    public HaDSOutfit(Transform storedAsset) : base(storedAsset)
    {
        modelData = this.storedAsset.gameObject.GetComponent<ModelData>();
        if (!modelData) { Log.Error("No ModelData component was found during HaDS constructor."); return; }
        BuildVariants3();
    }

    private void BuildVariants()
    {
        if (modelData?.accessories is null) { Log.Error("mda was null during build variants"); return; }
        if (Accessories is null) { Log.Error("accessories was null during build variants"); return; }
        Log.Debug($"{DisplayName}.BuildVariants()");
        HashSet<StoredAccessory> toggleables = new();
        foreach (var mda in modelData.accessories)
        {
            //Log.Debug($"Creating {DisplayName}.{mda.name}");
            List<StoredAccessory> thisVariant = new();
            if (mda?.objects is null) { Variants[mda.name] = thisVariant; continue; }
            foreach (var accessory in mda.objects)
            {
                if (!accessory) { continue; }
                var storedAcc = GetAccessory(accessory.name);
                if (storedAcc is null) continue;
                thisVariant.Add(storedAcc);
                toggleables.Add(storedAcc);
            }
            Variants[mda.name] = thisVariant;
        }
        //Log.Debug("toggleables created");
        var nonToggleables = Accessories.Where(x => !toggleables.Contains(x));
        //Log.Debug("non-toggleables created");
        foreach (var mda in modelData.accessories)
        {
            Variants[mda.name].AddRange(nonToggleables);
        }
        Log.Debug("Variants dict complete. ");
    }

    private void BuildVariants3()
    {
        foreach (int i in Range(0, modelData.accessories.Count))
        {
            if (compData. coopToggles.Count() == 0)
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

    private List<StoredAccessory> BuildVariant(int accessoryGroup, int coopToggle)
    {
        var results = Accessories.ToDictionary(x=>x, y=>true);

        Log.Debug("setting acc variants");
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

        Log.Debug("setting coop variants");
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