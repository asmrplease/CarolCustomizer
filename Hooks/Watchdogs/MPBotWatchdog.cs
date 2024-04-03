using CarolCustomizer.Behaviors;
using CarolCustomizer.Models;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs;
public class MPBotWatchdog : PelvisWatchdog
{
    VirtualCarol virtualCarol;

    public override PelvisWatchdog BuildFromExisting(PelvisWatchdog watchdog, Component typeComponent)
    {
        virtualCarol = typeComponent as VirtualCarol;
        return base.BuildFromExisting(watchdog, typeComponent);
    }

    public override void SetBaseOutfit(Outfit outfit)
    {

    }

    public void SetBotName(string botName)
    {
        VirtualCarol vCarol = virtualCarol;
        MultiplayerManager.PlayerStats stats = vCarol.GetPlayerStats();
        stats.name = botName;
    }
}
