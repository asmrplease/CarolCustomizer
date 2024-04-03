using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using CarolCustomizer.Behaviors;

namespace CarolCustomizer.Models;
public class RecipeDescriptor 
{
    public string BaseOutfitName;
    public int BaseAccessorySlot;
    public bool BaseVisible;
    public IEnumerable<AccessoryDescriptor> ActiveAccessories;
    public string Version;

    [JsonConstructor]
    public RecipeDescriptor(string baseOutfitName, int baseAccessorySlot, bool baseVisible, IEnumerable<AccessoryDescriptor> activeAccessories, string version) 
    {
        this.BaseOutfitName = baseOutfitName;
        this.BaseAccessorySlot = baseAccessorySlot;
        this.BaseVisible = baseVisible;
        this.ActiveAccessories = activeAccessories;
        this.Version = version;
    }

    public RecipeDescriptor(OutfitManager manager)
    {
        this.BaseOutfitName = manager.BaseOutfitName;
        this.BaseAccessorySlot = manager.BaseAccessorySlot;
        this.BaseVisible = manager.BaseVisible;
        this.ActiveAccessories = manager.ActiveAccessories.
            Select(x => new AccessoryDescriptor(x));
        this.Version = PluginInfo.PLUGIN_VERSION;
    }
}   