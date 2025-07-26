using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Models.Recipes;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs;
public class CampaignBot : MonoBehaviour, ICustomizable
{
    public PelvisWatchdog watchdog { get; private set; }

    void Awake()
    {
        SetBaseVisibility(false);
        NPCManager.OnBotSpawn(this);
    }

    public ICustomizable Constructor(PelvisWatchdog pelvisWatchdog)
    {
        this.watchdog = pelvisWatchdog;
        return this;
    }

    public void CustomizeBot(Recipe recipe, OutfitManager outfitManager) 
    {
        if (Settings.Plugin.customCampaignBots.Value is not true) return;

        RecipeApplier.ActivateRecipe(outfitManager, recipe.Descriptor);
    }

    public void SetBaseVisibility(bool visible)
    {
        if (Settings.Plugin.customCampaignBots.Value is not true) return;
        
        watchdog.CompData.SetBaseVisibility(visible);
    }

    void OnDestroy() => NPCManager.OnBotDespawn(watchdog);

    public void SetBaseOutfit(Outfit outfit)
    {
        throw new System.NotImplementedException();
    }

    public void SetAnimator(Outfit outfit)
    {
        throw new System.NotImplementedException();
    }

    public void SetHeightOffset(float height)
    {
        throw new System.NotImplementedException();
    }

    public void Dispose()
    {
        Destroy(this);
    }
}
