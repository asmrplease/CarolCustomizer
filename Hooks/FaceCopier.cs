using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CarolCustomizer.Utils;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Behaviors.Carol;

namespace CarolCustomizer.Hooks;
public class FaceCopier : MonoBehaviour
{
    #region Dependencies
    CarolInstance playerManager;
    #endregion

    #region Lifecycle
    public void Constructor(CarolInstance playerManager)
    {
        this.playerManager = playerManager;
        this.playerManager.SpawnEvent += UpdateBaseFace;
    }

    private void OnDestroy()
    {
        playerManager.SpawnEvent -= UpdateBaseFace;
    }
    #endregion

    #region Source Management
    SkinnedMeshRenderer baseFace;
    public void UpdateBaseFace(PelvisWatchdog watchdog)
    {
        this.baseFace = watchdog.CompData.BaseFace;
    }
    #endregion

    #region Target Management
    HashSet<LiveFace> targets = new();
    public void AddTarget(LiveFace target) => targets.Add(target);

    public void RemoveTarget(LiveFace target)
    {
        if (targets.Contains(target)) targets.Remove(target);
    }
    #endregion

    #region Apply Source to Targets
    private void Update()
    {
        if (!baseFace) return;
        float[] values = baseFace.GetAllBlendshapes();
        
        foreach (var target in targets.Where(x=>x.isActive))
        {
            target.SetAllBlendshapes(values);
        }
    }
    #endregion
}
