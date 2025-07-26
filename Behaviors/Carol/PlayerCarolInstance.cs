using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Utils;
using System;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Carol;
public class PlayerCarolInstance : CarolInstance
{
    PlayerModBehavior player;
    static Type playerWatchdogType = typeof(PlayerModBehavior);
    readonly AutoSaver autoSaver;
    public readonly int playerIndex;

    public bool Busy => player?.Busy ?? false;

    public PlayerCarolInstance(Transform folder, int playerIndex) : base(folder) 
    {
        this.playerIndex = playerIndex;
        autoSaver = new(this, playerIndex);
    }

    public override void NotifySpawned(PelvisWatchdog pelvis)
    {
        base.NotifySpawned(pelvis);
        if (!pelvis.Behavior.GetType().IsAssignableFrom(playerWatchdogType)) return;

        player = pelvis.Behavior as PlayerModBehavior;
    }

    public bool Exists() => player && player.enabled;

    public bool CanOpenMenu() => player?.CanOpenMenu() ?? true;

    public bool ManagesPlayer(Entity playerEntity) => player?.ManagesPlayer(playerEntity) ?? false;

    public void LockPlayer() => player?.LockPlayer();
    public void UnlockPlayer() => player?.UnlockPlayer();

    public override void Dispose()
    {
        Log.Debug("disposing PCI");
        autoSaver.Save();
        base.Dispose();
    }
}
