using CarolCustomizer.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CarolCustomizer.Utils;

namespace CarolCustomizer.Hooks.Watchdogs;
public class MenuWatchdog : PelvisWatchdog
{
    MenuSwitchOutfit menuSwitchOutfit;

    public override PelvisWatchdog BuildFromExisting(PelvisWatchdog watchdog, Component typeComponent)
    {
        Log.Debug("MenuWatchdog.Constructor");
        Log.Debug($"{watchdog.MeshData.baseMeshes.Count()}");
        Log.Debug($"ctor BoneData exists: {(bool)BoneData}");
        menuSwitchOutfit = typeComponent as MenuSwitchOutfit;
        return base.BuildFromExisting(watchdog, typeComponent);
    }

    void Start() => StartCoroutine(MainMenuFix());

    void OnEnable() => CCPlugin.cutscenePlayer.NotifySpawned(this);

    private IEnumerator MainMenuFix()
    {
        while (true)
        {
            if (MeshData.baseMeshes.Any(x => x.gameObject.activeSelf))
            {
                Log.Debug("MainMenuFix RefreshBaseVisibility");
                base.SetBaseVisibility(false);
            }
            yield return new WaitForSeconds(1);
        }
    }

    public override void SetBaseOutfit(Outfit outfit)
    {
        Log.Debug("SetBaseOutfit(MenuSwitchOutfit)");
        if (menuSwitchOutfit.loadedModel) { Destroy(menuSwitchOutfit.loadedModel); }
        menuSwitchOutfit.loadedModel = Util.SpawnOverTarget(outfit.storedAsset.gameObject, menuSwitchOutfit.gameObject);
        menuSwitchOutfit.loadedModel.SetActive(true);
        menuSwitchOutfit.loadedModel.transform.SetParent(menuSwitchOutfit.transform);
        menuSwitchOutfit.loadedModel.transform.ResetLocalPosRot();
        menuSwitchOutfit.modelData = ((HaDSOutfit)outfit).modelData;
        menuSwitchOutfit.PickHair(menuSwitchOutfit.transform);
    }
}
