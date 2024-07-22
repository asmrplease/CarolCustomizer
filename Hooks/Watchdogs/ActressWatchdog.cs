using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Utils;

namespace CarolCustomizer.Hooks.Watchdogs;
public class ActressWatchdog : PelvisWatchdog
{
    void OnEnable()
    {
        Log.Debug($"{this} OnEnable");
        SetBaseVisibility(false);
        PlayerInstances.DefaultPlayer.NotifySpawned(this);
    }

    void OnDisable()
    {
        Log.Debug("Restoring previous pelvis due to ActressWatchdog.OnDisable");
        PlayerInstances.DefaultPlayer?.RestorePrevious(this);
    }
}
