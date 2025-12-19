using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs.UnhandledArmatures;
internal class UnhandledArmature : MonoBehaviour, ICarolType
{
    protected PelvisWatchdog watchdog;
    public ICarolType Constructor(PelvisWatchdog watchdog)
    {
        Log.Info($"{gameObject.name} has an unhandled behavior type {this.GetType().Name}");
        this.watchdog = watchdog;
        this.watchdog.Behavior = this;
        return this;
    }

    void Warn(string methodName) => Log.Warning($"Tried to set {methodName} on {this.GetType().Name} armature.");

    public void SetAnimator(RuntimeAnimatorController rac) => Warn("animator");

    public void SetBaseOutfit(SourceDescriptor outfit) => Warn("base outfit");

    public void SetBaseVisibility(bool visibility) => Warn("visibility");

    public void SetHeightOffset(float height) => Warn("height offset");

    public void Dispose() => Destroy(this);
}