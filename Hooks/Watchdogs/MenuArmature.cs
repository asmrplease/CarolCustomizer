using System.Collections;
using System.Linq;
using UnityEngine;
using CarolCustomizer.Utils;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Contracts;
using Onirism.Ui;
using CarolCustomizer.Assets;

namespace CarolCustomizer.Hooks.Watchdogs;
public class MenuArmature : MonoBehaviour, ICarolType
{
    MenuSwitchOutfit menuSwitchOutfit;
    public PelvisWatchdog watchdog {  get; private set; }

    public ICarolType Constructor(PelvisWatchdog watchdog)
    {
        Log.Debug("MenuModBehavior.Constructor()");
        this.watchdog = watchdog;
        menuSwitchOutfit = this.gameObject.GetComponentInParent<MenuSwitchOutfit>(true);
        if (!menuSwitchOutfit) Log.Warning("No MenuSwitchOutfit component found!");
        return this;
    }

    //void Start() => StartCoroutine(MainMenuFix());

    void OnEnable()
    {
        Log.Info("MenuModBehavior.OnEnable()");
        this.watchdog = GetComponent<PelvisWatchdog>();
        this.watchdog.Behavior = this;
        this.menuSwitchOutfit = this.gameObject.GetComponentInParent<MenuSwitchOutfit>(true);
        SetBaseVisibility(false);
        PlayerInstances.DefaultPlayer.NotifySpawned(this.watchdog);
    }

    private IEnumerator MainMenuFix()
    {
        
        foreach(var i in Enumerable.Range(0,5))
        {
            if (watchdog.CompData.allSMRs.Any(x => x.gameObject.activeSelf))
            {
                //Log.Debug("MainMenuFix RefreshBaseVisibility");
                this.SetBaseVisibility(false);
            }
            yield return new WaitForSeconds(1);
        }
    }

    public void SetBaseOutfit(SourceDescriptor descriptor)
    {
        Log.Debug("SetBaseOutfit(MenuSwitchOutfit)");
        var source = OutfitAssetManager.GetAccessorySource(descriptor);
        if (source is not HaDSOutfit outfit) { Log.Warning($"SetBaseOutfit({descriptor}) found a source that was not an outfit"); return; }

        if (menuSwitchOutfit.loadedModel) { Destroy(menuSwitchOutfit.loadedModel); }
        menuSwitchOutfit.loadedModel = Util.SpawnOverTarget(outfit.storedAsset.gameObject, menuSwitchOutfit.gameObject);
        menuSwitchOutfit.loadedModel.SetActive(true);
        menuSwitchOutfit.loadedModel.transform.SetParent(menuSwitchOutfit.transform);
        menuSwitchOutfit.loadedModel.transform.ResetLocalPosRot();
        menuSwitchOutfit.modelData = outfit.modelData;
    }

    public void SetAnimator(RuntimeAnimatorController rac) { }

    public void SetHeightOffset(float height) { }

    public void SetBaseVisibility(bool visibility)
    {
        watchdog?.CompData?.SetBaseVisibility(visibility);
        GetComponentsInChildren<Hairstyle>()
            .ToList()
            .ForEach(x => GameObject.Destroy(x.gameObject));
    }

    public void Dispose()
    {
        Destroy(this);
    }
}
