using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Recipes;
using System;

namespace CarolCustomizer.Contracts;
public interface ICarolBot
{
    public void CustomizeBot(RecipeDescriptor recipe, OutfitCoordinator outfit, string name);
    public PelvisWatchdog Watchdog();
}
