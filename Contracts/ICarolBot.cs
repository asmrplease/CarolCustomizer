using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Recipes;
using System;

namespace CarolCustomizer.Contracts;
public interface ICarolBot
{
    public void CustomizeBot(Recipe recipe, OutfitCoordinator outfit);
    public PelvisWatchdog Watchdog();
}
