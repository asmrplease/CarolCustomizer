using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Carol;
public class PlayerCarolInstance : CarolInstance
{
    PlayerArmature player;
    static Type playerType = typeof(PlayerArmature);
    public readonly AutoSaver autoSaver;
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
        if (!pelvis.Behavior.GetType().IsAssignableFrom(playerType)) return;

        player = pelvis.Behavior as PlayerArmature;
    }

    public bool Exists() => player && player.enabled;

    public bool CanOpenMenu() => player?.CanOpenMenu() ?? true;

    public bool ManagesPlayer(Entity playerEntity) => player?.ManagesPlayer(playerEntity) ?? false;

    public void LockPlayer() => player?.LockPlayer();
    public void UnlockPlayer() => player?.UnlockPlayer();

    public override bool RestorePrevious(PelvisWatchdog pelvis)
    {
        return base.RestorePrevious(pelvis);
        var baseWorked = base.RestorePrevious(pelvis);
        if (baseWorked) return true;

        return FixMissing();
    }

    bool FixMissing()
    {
        //find a carolcontroller whose player number matches this instance's player number
        var found = Resources.FindObjectsOfTypeAll<CarolController>()
            .Where(x => x.playerNumber == this.playerIndex)
            .FirstOrDefault();
        if (!found) { Log.Error("Tried to find missing Player Carol Armature but no carol controllers were found."); return false; }

        //if we found a carolcontroller, get its watchdog
        var watchdog = found.GetComponentsInChildren<PelvisWatchdog>(true).FirstOrDefault();
        if (!watchdog) { Log.Error("Found missing CarolController, but it had no watchdog."); return false; }

        Log.Info("Found missing watchdog, notifying spawn");
        NotifySpawned(watchdog);
        return true;
    }

    public override void Dispose()
    {
        Log.Debug("disposing PCI");
        autoSaver.Save();
        base.Dispose();
    }
}
