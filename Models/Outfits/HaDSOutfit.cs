﻿using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        BuildVariants();
    }

    //How do we change the behavior to recognize player variants?
        //coopmodeltoggle has data about which player it's associated with
        //every coopmodeltoggle has a set of objects that change state with the toggle
            //typically these objects are
    //How do we present these options to the player?
        //list of each permutation, 4 players, 2 acc states = 8 variants
            //this probably isn't too bad, might have an issue with getting cut off near screen edges
        //player color preference setting?

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
