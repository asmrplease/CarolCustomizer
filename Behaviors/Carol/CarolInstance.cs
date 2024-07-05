using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CarolCustomizer.Utils;
using CarolCustomizer.Hooks.Watchdogs;

namespace CarolCustomizer.Behaviors.Carol;
/// <summary>
/// This class is responsible for one player or other type of carol through the lifetime of the plugin.
/// It delegates actions to other classes to maintain clothing continuity.
/// </summary>
public class CarolInstance : IDisposable
{
    #region Dependencies
    SkeletonManager skeletonManager;
    public OutfitManager outfitManager { get; private set; }
    #endregion

    #region WatchDog Instances
    PelvisWatchdog targetPelvis;
    List<PelvisWatchdog> previousTargets = new();
    #endregion

    #region Events
    public event Action<PelvisWatchdog> SpawnEvent;
    #endregion

    #region Lifecycle
    public CarolInstance(GameObject parent)
    {
        skeletonManager = new(this, parent);
        outfitManager = new(this, skeletonManager);
    }

    public virtual void Dispose()
    {
        skeletonManager.Dispose();
    }
    #endregion

    #region Notifications
    public virtual void NotifySpawned(PelvisWatchdog pelvis)
    {
        if (!pelvis) { Log.Error("Null pelviswatchdog on NotifySpawned()"); return; }
        if (pelvis == targetPelvis) { Log.Warning("PlayerManager was given its existing pelvis as a target."); return; }

        if (targetPelvis) { previousTargets.Add(targetPelvis); }
        previousTargets.RemoveAll(x => !x);
        targetPelvis = pelvis;

        Log.Debug($"SpawnEvent.Invoke({pelvis})");
        SpawnEvent?.Invoke(targetPelvis);
    }

    public bool RestorePrevious(PelvisWatchdog pelvis)
    {
        if (targetPelvis != pelvis) return false;
        if (previousTargets is null) return false;
        if (!previousTargets.Any(x => x)) return false;
        Log.Debug("Notifying last valid target of spawn");
        NotifySpawned(previousTargets.Last(x => x));
        return true;
    }
    #endregion
}
