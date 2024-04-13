using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CarolCustomizer.Utils;

namespace CarolCustomizer.Models;
public class HaDSOutfit : Outfit
{
    #region ModelData Handling 
    public override Sprite Sprite => modelData.portraitShop;

    public ModelData modelData { get; protected set; }

    public Dictionary<string, List<StoredAccessory>> Variants { get; private set; } = new();

    public HaDSOutfit(Transform storedAsset) : base(storedAsset)
    {
        this.modelData = this.storedAsset.gameObject.GetComponent<ModelData>();
        if (!this.modelData) { Log.Error("No ModelData component was found during HaDS constructor."); return; }
        BuildVariants();
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
    #endregion
}
