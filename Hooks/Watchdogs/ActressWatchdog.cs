using CarolCustomizer.Utils;

namespace CarolCustomizer.Hooks.Watchdogs;
public class ActressWatchdog : PelvisWatchdog
{
    void OnEnable()
    {
        Log.Debug($"{this} OnEnable");
        CCPlugin.cutscenePlayer.NotifySpawned(this);
    }

    void OnDisable()
    {
        Log.Debug("Restoring previous pelvis due to ActressWatchdog.OnDisable");
        CCPlugin.cutscenePlayer?.RestorePrevious(this);
    }
}
