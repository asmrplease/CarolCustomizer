using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Models.Outfits;

namespace CarolCustomizer.Hooks.Watchdogs;
internal class NPCWatchdog : PelvisWatchdog
{
    bool customNPCsEnabled;
    public NPC npcType { get; private set; }
    public override void Awake()
    {
        base.Awake();
        customNPCsEnabled = Settings.Plugin.customShezara.Value;
    }

    public override void SetBaseVisibility(bool visible)
    {
        if (customNPCsEnabled is not true) return;
        base.SetBaseVisibility(visible);
    }

    public override void SetAnimator(Outfit outfit) { }

    void OnEnable() 
    {
        if (!customNPCsEnabled) return;

        SetBaseVisibility(false);
        NPCManager.OnNPCAwake(this); 
    }

    void OnDestroy() 
    { 
        if (customNPCsEnabled) NPCManager.NPCs[npcType].RestorePrevious(this); 
    }
}
