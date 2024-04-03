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
    public HaDSOutfit(Transform storedAsset) : base(storedAsset)
    {
        this.modelData = this.storedAsset.gameObject.GetComponent<ModelData>();
        if (!this.modelData) { Log.Error("No ModelData component was found during HaDS constructor."); }
    }
    #endregion
}
