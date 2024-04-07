using CarolCustomizer.Behaviors;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarolCustomizer.Hooks.Watchdogs;
public class BotWatchdog : PelvisWatchdog
{
    public override void Awake()
    {
        base.Awake();
        NPCManager.OnBotSpawn(this);
    }
    public virtual void SetBotName(string botName) { }

    void OnDestroy() => NPCManager.OnBotDespawn(this);
}
