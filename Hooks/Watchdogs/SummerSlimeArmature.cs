using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs;
public class SummerSlimeArmature : MonoBehaviour, ICarolType, ICarolBot
{
    public PelvisWatchdog watchdog { get; private set; }
    public PelvisWatchdog Watchdog() => watchdog;

    static int spawnCount = 0;
    public bool custom { get; private set; }

    void Awake()
    {
        this.watchdog = PelvisWatchdog.GetAddWatchdog(this.gameObject);
        if (!watchdog) { Log.Error("Failed to get watchdog during SummerSlime.Awake()"); return; }

        spawnCount += 1; spawnCount %= 10;
        custom = spawnCount <= Settings.Plugin.customSummerSlimes.Value;

        this.watchdog.Behavior = this;
        if (custom is not true) return;

        NPCManager.OnBotSpawn(this);
    }

    public ICarolType Constructor(PelvisWatchdog pelvisWatchdog)
    {
        return this;
    }

    public void CustomizeBot(Recipe recipe, OutfitManager outfitManager) 
    {
        if (!custom) return;

        SetBaseVisibility(false);
        RecipeApplier.ActivateRecipe(outfitManager, recipe.Descriptor);
        var swimsuit = this.transform.parent.Find("Slimeswimsuit");
        if (!swimsuit) { Log.Warning("Failed to find Slimeswimsuit during SummerSlime.CustomizeBot()"); return; }

        var smr = swimsuit.GetComponent<SkinnedMeshRenderer>();
        if (!smr) { Log.Warning("Failed to find swimsuit SMR during SumemrSlime.CustomizeBot()"); return; }

        foreach (var acc in outfitManager.ActiveAccessories)
        {
            outfitManager.PaintAccessoryShared(acc, acc.Materials.Select(x => smr.material).ToList());
        }
        outfitManager.SetHairColor(smr.material);
    }

    public void SetBaseVisibility(bool visible)
    {
        if (!custom) return;
        
        GetComponent<CompData>().SetBaseVisibility(visible);
        var randomizer = GetComponent<ZombieRandomizer>();
        if (!randomizer) { Log.Warning("Failed to find ZombieRandomizer in SummerSlime.SetBaseVisibility()"); return; }

        randomizer.enabled = visible;
    }

    void OnDestroy() 
    {
        Log.Debug("SummerSlime.OnDestroy()");
        NPCManager.OnBotDespawn(this);
    }

    public void SetBaseOutfit(Outfit outfit) { }

    public void SetAnimator(Outfit outfit) { }

    public void SetHeightOffset(float height) { }

    public void Dispose() => Destroy(this);
}
