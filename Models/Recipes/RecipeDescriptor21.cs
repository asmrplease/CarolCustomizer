using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace CarolCustomizer.Models.Recipes;
public class RecipeDescriptor21
{
    public string AnimatorSource;
    public string BaseOutfitName;
    public IEnumerable<AccessoryDescriptor> ActiveAccessories;
    public string Version;

    [JsonConstructor]
    public RecipeDescriptor21(string animatorSource, string baseOutfitName, IEnumerable<AccessoryDescriptor> activeAccessories, string version)
    {
        AnimatorSource = animatorSource;
        BaseOutfitName = baseOutfitName;
        ActiveAccessories = activeAccessories;
        Version = version;
    }

    public RecipeDescriptor21(OutfitManager manager)
    {
        BaseOutfitName = Constants.Pyjamas;
        AnimatorSource = manager.AnimatorSource;
        ActiveAccessories = manager.ActiveAccessories
            .Select(x => new AccessoryDescriptor(x));
        Version = PluginInfo.PLUGIN_VERSION;
    }
}
