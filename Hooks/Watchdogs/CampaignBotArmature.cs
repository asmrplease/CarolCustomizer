using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Models.Recipes;
using System;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs;
public class CampaignBotArmature : MonoBehaviour, ICarolType, ICarolBot
{
    public PelvisWatchdog watchdog { get; private set; }
    public PelvisWatchdog Watchdog() => watchdog;

    Guid id = Guid.NewGuid();

    public Guid ID() => id;
    void Awake()
    {
        this.watchdog = GetComponent<PelvisWatchdog>();
        this.watchdog.Behavior = this;
        SetBaseVisibility(false);
        NPCManager.OnBotSpawn(this);
    }

    public ICarolType Constructor(PelvisWatchdog pelvisWatchdog)
    {
        this.watchdog = pelvisWatchdog;
        return this;
    }

    public void CustomizeBot(Recipe recipe, OutfitCoordinator outfitManager) 
    {
        if (Settings.Plugin.customCampaignBots.Value is not true) return;

        RecipeApplier.ActivateRecipe(outfitManager, recipe.Descriptor);
    }

    public void SetBaseVisibility(bool visible)
    {
        if (Settings.Plugin.customCampaignBots.Value is not true) return;
        
        watchdog.CompData.SetBaseVisibility(visible);
    }

    void OnDestroy() => NPCManager.OnBotDespawn(this);

    public void SetBaseOutfit(Outfit outfit)
    {
        
    }

    public void SetAnimator(Outfit outfit)
    {
        
    }

    public void SetHeightOffset(float height)
    {
        
    }

    public void Dispose()
    {
        Destroy(this);
    }
}
