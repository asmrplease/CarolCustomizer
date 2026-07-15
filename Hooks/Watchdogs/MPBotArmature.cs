using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Utils;
using System;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs;
public class MPBotArmature : MonoBehaviour, ICarolType, ICustomizable
{
    VirtualCarol virtualCarol;
    public PelvisWatchdog watchdog { get; private set; }

    public PelvisWatchdog Watchdog() => watchdog;

    Guid id = Guid.NewGuid();

    public Guid ID() => id;

    public void SetBaseVisibility(bool visible)
    {
        if (Settings.Plugin.customMPBots.Value is not true) return;

        Log.Debug($"MPBotBehavior.SetBaseVisibility({visible})");
        watchdog.CompData.SetBaseVisibility(visible);
    }

    public void Customize(RecipeDescriptor recipe, OutfitCoordinator outfit, string name)
    {
        if (Settings.Plugin.customMPBots.Value is not true) return;

        RecipeApplier.ActivateRecipe(outfit, recipe);
        SetMPName(name);
    }

    void SetMPName(string name)
    {
        if (name is null) { Log.Debug("SetBotName passed null name"); return; }

        virtualCarol ??= GetComponentInParent<VirtualCarol>(true);
        if (!virtualCarol) { Log.Warning("VirtualCarol null during SetBotName"); return; }

        //MultiplayerManager.PlayerStats stats = virtualCarol?.GetPlayerStats();
        //if (stats is null) { Log.Error("didn't find stats from virtualCarol"); return; }

        //stats.name = name;
        Log.Warning("SetMPName was commented out due to a programming change and never restored.");
    }

    public ICarolType Constructor(PelvisWatchdog watchdog)
    {
        this.watchdog = watchdog;
        this.watchdog.Behavior = this;
        virtualCarol ??= GetComponentInParent<VirtualCarol>(true);
        this.watchdog.AnimData.Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        SetBaseVisibility(false);
        NPCManager.OnBotSpawn(this);
        return this;
    }

    public void SetBaseOutfit(SourceDescriptor outfit) { }

    public void SetAnimator(RuntimeAnimatorController rac) { }

    public void SetHeightOffset(float height) { }

    public void Dispose()
    {
        NPCManager.OnBotDespawn(this);
        Destroy(this);
    }
}