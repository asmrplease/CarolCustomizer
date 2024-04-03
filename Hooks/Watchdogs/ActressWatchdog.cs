using CarolCustomizer.Behaviors;
using CarolCustomizer.Models;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs;
public class ActressWatchdog : PelvisWatchdog
{
    //protected override void Start() { }

    void OnEnable()
    {
        Log.Debug($"{this} OnEnable");
        CCPlugin.cutscenePlayer.NotifySpawned(this);
    }

    public override void SetBaseOutfit(Outfit outfit)
    {
        //TODO: how do we implement this?
    }

    void OnDisable()
    {
        Log.Debug("Restoring previous pelvis due to ActressWatchdog.OnDisable");
        CCPlugin.cutscenePlayer?.RestorePrevious(this);
    }
}
