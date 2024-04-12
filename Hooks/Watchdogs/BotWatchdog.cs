﻿using CarolCustomizer.Behaviors;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CarolCustomizer.Hooks.Watchdogs;
public class BotWatchdog : PelvisWatchdog
{
    public override void Awake()
    {
        base.Awake();
        NPCManager.OnBotSpawn(this);
    }
    public virtual void SetBotName(string botName) { Log.Debug("BotWatchdog.SetBotName()"); }

    public override void SetBaseVisibilty(bool visible)
    {
        foreach (var mesh in MeshData?.baseMeshes.Where(x=>x.name != Constants.RobotHead)) 
            { mesh.gameObject.SetActive(visible); }
    }

    void OnDestroy() => NPCManager.OnBotDespawn(this);
}
