using CarolCustomizer.Behaviors;
using CarolCustomizer.Models;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs;
public class MPBotWatchdog : BotWatchdog
{
    VirtualCarol virtualCarol;
    Entity entity;

    public override PelvisWatchdog BuildFromExisting(PelvisWatchdog watchdog, Component typeComponent)
    {
        virtualCarol = typeComponent as VirtualCarol;
        return base.BuildFromExisting(watchdog, typeComponent);
    }

    public override void SetBaseOutfit(Outfit outfit) 
    {
        entity ??= GetComponentInParent<Entity>(true);
        entity.SwapModel(outfit.storedAsset.gameObject);
    }

    public override void SetBotName(string botName)
    {
        MultiplayerManager.PlayerStats stats = virtualCarol.GetPlayerStats();
        stats.name = botName;
    }
}
