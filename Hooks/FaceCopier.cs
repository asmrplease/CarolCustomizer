using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CarolCustomizer.Utils;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Assets;
using CarolCustomizer.Models.Outfits;

namespace CarolCustomizer.Hooks;
public class FaceCopier : MonoBehaviour
{
    #region Dependencies
    CarolInstance playerManager;
    #endregion

    HashSet<LiveFace> targets = new();
    SkinnedMeshRenderer baseFace;

    #region Lifecycle
    public FaceCopier Constructor(CarolInstance playerManager)
    {
        this.playerManager = playerManager;
        this.playerManager.SpawnEvent += UpdateBaseFace;
        OutfitAssetManager.OnOutfitUnloaded += OnOutfitUnloaded;
        return this;
    }

    void OnOutfitUnloaded(Outfit outfit)
    {
        targets.RemoveWhere(x => x is null || x.outfit == outfit);
    }

    void OnDestroy()
    {
        playerManager.SpawnEvent -= UpdateBaseFace;
    }
    #endregion

    #region Source Management
    public void UpdateBaseFace(PelvisWatchdog watchdog)
    {
        this.baseFace = watchdog.CompData.BaseFace;
    }
    #endregion

    #region Target Management
    public void AddTarget(LiveFace target) => targets.Add(target);

    public void RemoveTarget(LiveFace target)
    {
        if (targets.Contains(target)) targets.Remove(target);
    }
    #endregion

    #region Apply Source to Targets
    void Update()
    {
        if (!baseFace) return;
        float[] values = baseFace.GetAllBlendshapes();
        
        foreach (var target in targets)
        {
            if (target is null) continue;
            target.SetAllBlendshapes(values);
        }

        //TODO: would not using linq here be more performant?
        //foreach (var target in targets.Where(x=> x is not null && x.isActive))
        //{
        //    target.SetAllBlendshapes(values);
        //}
    }
    #endregion
}
