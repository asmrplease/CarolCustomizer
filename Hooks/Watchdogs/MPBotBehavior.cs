using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Utils;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs;
public class MPBotBehavior : MonoBehaviour, ICustomizable
{
    VirtualCarol virtualCarol;
    public PelvisWatchdog watchdog { get; private set; }

    public void SetBaseVisibility(bool visible)
    {
        if (Settings.Plugin.customMPBots.Value is not true) return;

        watchdog.CompData.SetBaseVisibility(visible);
    }

    public void CustomizeBot(Recipe recipe, OutfitManager outfit)
    {
        if (!Settings.Plugin.customMPBots.Value) return;

        RecipeApplier.ActivateRecipe(outfit, recipe.Descriptor);
        SetMPName(recipe.Name);
    }

    void SetMPName(string name)
    {
        if (name is null) { Log.Debug("SetBotName passed null name"); return; }

        virtualCarol ??= GetComponentInParent<VirtualCarol>(true);
        if (!virtualCarol) { Log.Warning("VirtualCarol null during SetBotName"); return; }

        MultiplayerManager.PlayerStats stats = virtualCarol?.GetPlayerStats();
        if (stats is null) { Log.Error("didn't find stats from virtualCarol"); return; }

        stats.name = name;
    }

    public ICustomizable Constructor(PelvisWatchdog watchdog)
    {
        this.watchdog = watchdog;
        virtualCarol ??= GetComponentInParent<VirtualCarol>(true);
        this.watchdog.AnimData.Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        return this;
    }

    public void SetBaseOutfit(Outfit outfit) { }

    public void SetAnimator(Outfit outfit) { }

    public void SetHeightOffset(float height) { }

    public void Dispose()
    {
        Destroy(this);
    }
}