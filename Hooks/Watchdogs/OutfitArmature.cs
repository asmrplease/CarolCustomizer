using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs;
internal class OutfitArmature : MonoBehaviour, ICarolType
{
    PelvisWatchdog watchdog;
    public ICarolType Constructor(PelvisWatchdog watchdog)
    {
        this.watchdog = watchdog;
        this.watchdog.Behavior = this;
        return this;
    }

    public void SetAnimator(Outfit outfit)
    {
        Log.Error("Tried to set animator on an outfit reference pelvis.");
    }

    public void SetBaseOutfit(Outfit outfit)
    {
        Log.Error("Tried to set base outfit on an outfit reference pelvis.");
    }

    public void SetBaseVisibility(bool visibility)
    {
        Log.Error("Tried to set base visibility on an outfit reference pelvis.");
    }

    public void SetHeightOffset(float height)
    {
        Log.Error("Tried to set height offset on an outfit reference pelvis.");
    }

    public void Dispose()
    {
        Destroy(this);
    }
}
