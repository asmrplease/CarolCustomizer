using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Models.Outfits;
using System.Linq;

namespace CarolCustomizer.Hooks.Watchdogs;
internal class PirateWatchdog : PelvisWatchdog
{
    bool pirateEnabled;
    public override void Awake()
    {
        base.Awake();
        pirateEnabled = Settings.Plugin.customShezara.Value;
    }

    public override void SetBaseVisibility(bool visible)
    {
        if (pirateEnabled is not true) return;
        base.SetBaseVisibility(visible);
    }

    public override void SetAnimator(Outfit outfit) { }

    void OnEnable() { if (pirateEnabled) NPCManager.OnShezaraAwake(this); }

    void OnDestroy() { if (pirateEnabled) NPCManager.shezaraInstance?.RestorePrevious(this); }
}
