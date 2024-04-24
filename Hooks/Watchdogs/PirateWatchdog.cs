using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Settings;

namespace CarolCustomizer.Hooks.Watchdogs;
internal class PirateWatchdog : PelvisWatchdog
{
    bool pirateEnabled;
    public override void Awake()
    {
        base.Awake();
        pirateEnabled = Settings.Plugin.customShezara.Value;
    }

    void OnEnable() { if (pirateEnabled) NPCManager.OnShezaraAwake(this); }

    void OnDestory() { if (pirateEnabled) NPCManager.shezaraInstance?.RestorePrevious(this); }
}
