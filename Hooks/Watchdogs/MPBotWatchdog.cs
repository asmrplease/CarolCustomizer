using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Utils;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs;
public class MPBotWatchdog : BotWatchdog
{
    VirtualCarol virtualCarol;

    public override PelvisWatchdog BuildFromExisting(PelvisWatchdog watchdog, Component typeComponent)
    {
        virtualCarol = typeComponent as VirtualCarol;
        return base.BuildFromExisting(watchdog, typeComponent);
    }

    public override void SetBaseVisibility(bool visible)
    {
        if (Settings.Plugin.customMPBots.Value is not true) return;
        if (!CompData || CompData.allSMRs is null) return;

        CompData.allSMRs
            .Where(x => x)
            .ForEach(mesh => mesh.gameObject.SetActive(visible));
    }

    public override void CustomizeBot(Recipe recipe, OutfitManager outfit)
    {
        if (!Settings.Plugin.customMPBots.Value) return;
        RecipeApplier.ActivateRecipe(outfit, recipe.Descriptor);
        SetMPName(recipe.Name);
    }

    private void SetMPName(string name)
    {
        if (name is null) { Log.Debug("SetBotName passed null name"); return; }

        virtualCarol ??= GetComponentInParent<VirtualCarol>(true);
        if (!virtualCarol) { Log.Warning("VirtualCarol null during SetBotName"); return; }

        MultiplayerManager.PlayerStats stats = virtualCarol?.GetPlayerStats();
        if (stats is null) { Log.Error("didn't find stats from virtualCarol"); return; }

        stats.name = name;
    }
}