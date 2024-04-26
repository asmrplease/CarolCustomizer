using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Models.Outfits;

namespace CarolCustomizer.Hooks.Watchdogs;
internal class PirateWatchdog : PelvisWatchdog
{
    bool pirateEnabled;
    public override void Awake()
    {
        base.Awake();
        pirateEnabled = Settings.Plugin.customShezara.Value;
    }

    public override void SetAnimator(Outfit outfit) { }

    void OnEnable() { if (pirateEnabled) NPCManager.OnShezaraAwake(this); }

    void OnDestroy() { if (pirateEnabled) NPCManager.shezaraInstance?.RestorePrevious(this); }
}
