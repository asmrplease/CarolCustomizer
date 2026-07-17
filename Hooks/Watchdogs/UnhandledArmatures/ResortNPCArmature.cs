using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs.UnhandledArmatures;
internal class ResortNPCArmature : MonoBehaviour, ICarolType, ICustomizable
{
    PelvisWatchdog watchdog;

    void Awake()
    {
        this.watchdog = PelvisWatchdog.GetAddWatchdog(this.gameObject);
        if (!watchdog) { Log.Error("Failed to get watchdog during WitchArmature.Awake()"); return; }

        this.watchdog.Behavior = this;

        NPCManager.OnBotSpawn(this);
    }

    public void Customize(RecipeDescriptor recipe, OutfitCoordinator outfitManager, string name)
    {
        SetBaseVisibility(false);
        RecipeApplier.ActivateRecipe(outfitManager, recipe);
    }

    public PelvisWatchdog Watchdog() => watchdog;

    public ICarolType Constructor(PelvisWatchdog watchdog)
    {
        //throw new System.NotImplementedException();
        return this;
    }

    public void SetBaseOutfit(SourceDescriptor outfit)
    {
        //throw new System.NotImplementedException();
    }

    public void SetAnimator(RuntimeAnimatorController rac)
    {
        //throw new System.NotImplementedException();
    }

    public void SetHeightOffset(float height)
    {
        //throw new System.NotImplementedException();
    }

    public void SetBaseVisibility(bool visibility)
    {
        //throw new System.NotImplementedException();
    }

    public void Dispose()
    {
        //throw new System.NotImplementedException();
    }
}

