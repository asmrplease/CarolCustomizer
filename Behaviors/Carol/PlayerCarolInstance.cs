using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Utils;
using System;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Carol;
public class PlayerCarolInstance : CarolInstance
{
    PlayerWatchdog player;
    static Type playerWatchdogType = typeof(PlayerWatchdog);
    AutoSaver autoSaver;
    public PlayerCarolInstance(GameObject parent) : base(parent)
    {
        autoSaver = new(this);
    }

    public override void NotifySpawned(PelvisWatchdog pelvis)
    {
        base.NotifySpawned(pelvis);
        //TODO: ew reflection
        if (!pelvis.GetType().IsAssignableFrom(playerWatchdogType)) return;
        player = pelvis as PlayerWatchdog;
    }

    public bool CanOpenMenu() => player?.CanOpenMenu() ?? true;

    public bool ManagesPlayer(Entity playerEntity)
        => player ? player.ManagesPlayer(playerEntity) : false;

    public void LockPlayer() => player?.LockPlayer();
    public void UnlockPlayer() => player?.UnlockPlayer();

    public override void Dispose()
    {
        Log.Debug("autosaving...");
        autoSaver.Save();
        Log.Debug("disposing PCI");
        base.Dispose();
    }
}
