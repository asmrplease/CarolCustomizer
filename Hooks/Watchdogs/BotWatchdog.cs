using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Models.Recipes;

namespace CarolCustomizer.Hooks.Watchdogs;
public class BotWatchdog : PelvisWatchdog
{
    public override void Awake()
    {
        base.Awake();
        SetBaseVisibility(false);
        NPCManager.OnBotSpawn(this);
    }

    public virtual void CustomizeBot(Recipe recipe, OutfitManager outfitManager) 
    {
        if (Settings.Plugin.customCampaignBots.Value is not true) return;

        RecipeApplier.ActivateRecipe(outfitManager, recipe.Descriptor);
    }

    public override void SetBaseVisibility(bool visible)
    {
        if (Settings.Plugin.customCampaignBots.Value is not true) return;
        
        base.SetBaseVisibility(visible);
    }

    void OnDestroy() => NPCManager.OnBotDespawn(this);
}
