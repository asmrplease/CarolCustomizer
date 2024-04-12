using CarolCustomizer.Utils;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs;
public class MPBotWatchdog : BotWatchdog
{
    VirtualCarol virtualCarol;

    public override PelvisWatchdog BuildFromExisting(PelvisWatchdog watchdog, Component typeComponent)
    {
        virtualCarol = typeComponent as VirtualCarol;
        return base.BuildFromExisting(watchdog, typeComponent);
    }

    public override void SetBotName(string botName)
    {
        if (botName is null) { Log.Debug("SetBotName passed null name"); return; }
        virtualCarol ??= GetComponentInParent<VirtualCarol>(true);
        if (!virtualCarol) { Log.Warning("VirtualCarol null during SetBotName"); return; }
        MultiplayerManager.PlayerStats stats = virtualCarol?.GetPlayerStats();
        if (stats is null) { Log.Error("didn't find stats from virtualCarol"); return; }
        stats.name = botName;
    }
}