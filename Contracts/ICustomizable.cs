using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Hooks.Watchdogs;

namespace CarolCustomizer.Contracts;
public interface ICustomizable
{
    public void Customize(RecipeDescriptor recipe, OutfitCoordinator outfit, string name);
    public PelvisWatchdog Watchdog();
}
