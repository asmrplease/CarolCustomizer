using CarolCustomizer.Assets;
using CarolCustomizer.Hooks;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Carol;
/// <summary>
/// This class is responsible for one player or other type of carol through the lifetime of the plugin.
/// It delegates actions to other classes to maintain clothing continuity.
/// </summary>
public class CarolInstance : IDisposable
{
    #region Dependencies
    public OutfitCoordinator outfitManager { get; private set; }
    #endregion

    #region WatchDog Instances
    PelvisWatchdog targetPelvis;
    List<PelvisWatchdog> previousTargets = [];
    #endregion

    #region Events
    public event Action<PelvisWatchdog> SpawnEvent;
    #endregion

    #region Lifecycle
    public CarolInstance(Transform parent)
    {
        outfitManager = new(this, parent.gameObject);
    }

    public virtual void Dispose()
    {
        Log.Debug("CarolInstance.Dispose()");
        outfitManager.Dispose();
        if (targetPelvis) GameObject.Destroy(targetPelvis);
    }
    #endregion

    #region Notifications
    public virtual void NotifySpawned(PelvisWatchdog pelvis)
    {
        if (SceneResourceProvider.Loading) { Log.Info("Ignoring pelvis spawns due to SceneResourceProvider.Loading == true"); return; }
        if (!pelvis) { Log.Error("Null pelviswatchdog on NotifySpawned()"); return; }
        if (pelvis == targetPelvis) { Log.Warning("PlayerManager was given its existing pelvis as a target."); return; }
        if (pelvis.Behavior is UnknownArmature) { Log.Warning("Ignoring UnknownCarolBehavior."); return; }

        Log.Info("CarolInstance.NotifySpawned");
        if (targetPelvis) { previousTargets.Add(targetPelvis); }
        previousTargets.RemoveAll(x => !x);
        targetPelvis = pelvis;

        Log.Debug($"SpawnEvent.Invoke({pelvis})");
        SpawnEvent?.Invoke(targetPelvis);
    }

    public bool RestorePrevious(PelvisWatchdog pelvis)
    {
        if (SceneResourceProvider.Loading) return false;
        if (targetPelvis != pelvis) return false;
        if (previousTargets is null) return false;
        if (!previousTargets.Any(x => x)) return false;
        Log.Debug("Notifying last valid target of spawn");
        NotifySpawned(previousTargets.Last(x => x));
        return true;
    }
    #endregion
}
