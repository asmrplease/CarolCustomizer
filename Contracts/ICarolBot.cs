using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Recipes;

namespace CarolCustomizer.Contracts;
public interface ICarolBot
{
    public void CustomizeBot(Recipe recipe, OutfitManager outfit);
    public PelvisWatchdog Watchdog();
}
