using CarolCustomizer.Behaviors;

namespace CarolCustomizer.Hooks.Watchdogs;
internal class PirateWatchdog : PelvisWatchdog
{
    void OnEnable() => NPCManager.OnShezaraAwake(this);

    void OnDestory() => NPCManager.shezaraInstance?.RestorePrevious(this);
}
