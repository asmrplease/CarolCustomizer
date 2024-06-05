using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Utils;
using System.Linq;

namespace CarolCustomizer.Hooks.Watchdogs;
public class BotWatchdog : PelvisWatchdog
{
    public override void Awake()
    {
        base.Awake();
        SetBaseVisibility(false);
        NPCManager.OnBotSpawn(this);
    }

    public virtual void CustomizeBot(Recipe recipe, OutfitManager outfit) 
    {
        if (Settings.Plugin.customCampaignBots.Value is not true) return;

        RecipeApplier.ActivateRecipe(outfit, recipe.Descriptor);
    }

    public override void SetBaseVisibility(bool visible)
    {
        if (Settings.Plugin.customCampaignBots.Value is not true) return;

        foreach (var mesh in CompData?.allSMRs.Where(x=>x.name != Constants.RobotHead)) 
            { mesh.gameObject.SetActive(visible); }
    }

    void OnDestroy() => NPCManager.OnBotDespawn(this);
}
