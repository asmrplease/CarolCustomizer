using System.Collections;
using System.Linq;
using UnityEngine;
using CarolCustomizer.Utils;
using CarolCustomizer.Models.Outfits;

namespace CarolCustomizer.Hooks.Watchdogs;
public class MenuWatchdog : PelvisWatchdog
{
    MenuSwitchOutfit menuSwitchOutfit;

    public override PelvisWatchdog BuildFromExisting(PelvisWatchdog watchdog, Component typeComponent)
    {
        Log.Debug("MenuWatchdog.Constructor");
        Log.Debug($"{watchdog.CompData.allSMRs.Count()}");
        Log.Debug($"ctor BoneData exists: {(bool)BoneData}");
        menuSwitchOutfit = typeComponent as MenuSwitchOutfit;
        return base.BuildFromExisting(watchdog, typeComponent);
    }

    void Start() => StartCoroutine(MainMenuFix());

    void OnEnable()
    {
        SetBaseVisibility(false);
        CCPlugin.cutscenePlayer.NotifySpawned(this);
    }

    private IEnumerator MainMenuFix()
    {
        while (true)
        {
            if (CompData.allSMRs.Any(x => x.gameObject.activeSelf))
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
