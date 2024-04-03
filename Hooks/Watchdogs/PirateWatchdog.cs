using CarolCustomizer.Behaviors;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarolCustomizer.Hooks.Watchdogs;
internal class PirateWatchdog : PelvisWatchdog
{
    void OnEnable() => NPCManager.OnShezaraAwake(this);

    //void OnDisable() => NPCManager.shezaraInstance?.RestorePrevious();

    void OnDestory() => NPCManager.OnShezaraDestroyed(this);
}
