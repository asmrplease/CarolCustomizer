using CarolCustomizer.Behaviors.Carol
;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Outfits;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs;
public class NPCArmature : MonoBehaviour, ICarolType
{
    bool thisNPCEnabled;
    public NPC npcType { get; private set; }
    public PelvisWatchdog watchdog { get; private set; }

    public ICarolType Constructor(PelvisWatchdog watchdog)
    {
        this.watchdog = watchdog;
        this.npcType = NPCManager.GetNPCType(this.transform.parent.name);
        return this;
    }
    void Awake()
    {
        this.npcType = NPCManager.GetNPCType(this.transform.parent.name);

        thisNPCEnabled = Settings.Plugin.customNPCs[npcType].enable.Value;
    }

    public void SetBaseVisibility(bool visible) { }

    public void SetAnimator(Outfit outfit) { }

    void OnEnable() 
    {
        if (!thisNPCEnabled) return;

        this.watchdog = PelvisWatchdog.GetAddWatchdog(this.gameObject);
        this.watchdog.Behavior = this;
        this.npcType = NPCManager.GetNPCType(this.transform.parent.name);
        watchdog.CompData.SetBaseVisibility(false);
        NPCManager.OnNPCAwake(this); 
    }

    void OnDisable() => NPCManager.NPCs[npcType].RestorePrevious(watchdog);

    void OnDestroy() 
    { 
        NPCManager.NPCs[npcType].RestorePrevious(watchdog); 
    }

    public void SetBaseOutfit(Outfit outfit) { }

    public void SetHeightOffset(float height) { }

    public void Dispose()
    {
        Destroy(this);
    }
}
