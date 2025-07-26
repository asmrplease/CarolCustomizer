using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs;
public class CarolActressBehavior : MonoBehaviour, ICustomizable
{
    PelvisWatchdog watchdog;

    void OnEnable()
    {
        Log.Debug($"{this} OnEnable");
        watchdog = GetComponent<PelvisWatchdog>();
        watchdog.Behavior.SetBaseVisibility(false);
        PlayerInstances.DefaultPlayer.NotifySpawned(watchdog);
    }

    void OnDisable()
    {
        Log.Debug("Restoring previous pelvis due to ActressWatchdog.OnDisable");
        PlayerInstances.DefaultPlayer?.RestorePrevious(watchdog);
    }

    public ICustomizable Constructor(PelvisWatchdog watchdog)
    {
        this.watchdog = watchdog;
        return this;
    }

    public void SetBaseOutfit(Outfit outfit)
    {
        
    }

    public void SetAnimator(Outfit outfit)
    {
        
    }

    public void SetHeightOffset(float height)
    {
        
    }

    public void SetBaseVisibility(bool visibility)
    {
        watchdog.CompData.SetBaseVisibility(visibility);
    }

    public void Dispose()
    {
        Destroy(this);
    }
}
