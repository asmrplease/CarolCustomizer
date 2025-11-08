using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs;
internal class UnknownArmature : MonoBehaviour, ICarolType
{
    PelvisWatchdog watchdog;
    public ICarolType Constructor(PelvisWatchdog watchdog)
    {
        Log.Error($"{this.gameObject.name} has an unknown behavior type");
        this.watchdog = watchdog;
        this.watchdog.Behavior = this;
        return this;
    }

    public void SetAnimator(RuntimeAnimatorController rac)
    {
        Log.Error("Tried to set animator on an unknown carol type.");
    }

    public void SetBaseOutfit(SourceDescriptor outfit)
    {
        Log.Error("Tried to set base outfit on an unknown carol type.");
    }

    public void SetBaseVisibility(bool visibility)
    {
        Log.Error("Tried to set base visibility on an unknown carol type.");
    }

    public void SetHeightOffset(float height)
    {
        Log.Error("Tried to set height offset on an unknown carol type.");
    }

    public void Dispose()
    {
        Destroy(this);
    }
}
