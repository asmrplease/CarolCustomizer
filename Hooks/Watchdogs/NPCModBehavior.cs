using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Outfits;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs;
internal class NPCModBehavior : MonoBehaviour, ICustomizable
{
    bool customNPCsEnabled;
    public NPC npcType { get; private set; }
    public PelvisWatchdog watchdog { get; private set; }

    public ICustomizable Constructor(PelvisWatchdog watchdog)
    {
        this.watchdog = watchdog;
        this.npcType = NPCManager.GetNPCType(this.transform.parent.name);
        return this;
    }
    void Awake()
    {
        customNPCsEnabled = Settings.Plugin.customShezara.Value;
        this.npcType = NPCManager.GetNPCType(this.transform.parent.name);
    }

    public void SetBaseVisibility(bool visible)
    {
        if (customNPCsEnabled is not true) return;
        watchdog.CompData.SetBaseVisibility(visible);
    }

    public void SetAnimator(Outfit outfit) { }

    void OnEnable() 
    {
        if (!customNPCsEnabled) return;

        this.watchdog = GetComponent<PelvisWatchdog>();
        SetBaseVisibility(false);
        this.npcType = NPCManager.GetNPCType(this.transform.parent.name);
        NPCManager.OnNPCAwake(this); 
    }

    void OnDestroy() 
    { 
        if (customNPCsEnabled) NPCManager.NPCs[npcType].RestorePrevious(watchdog); 
    }

    public void SetBaseOutfit(Outfit outfit)
    {
        
    }

    public void SetHeightOffset(float height)
    {
        
    }

    public void Dispose()
    {
        Destroy(this);
    }
}
