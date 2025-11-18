using CarolCustomizer.Assets;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs;
public class ActressArmature : MonoBehaviour, ICarolType
{
    PelvisWatchdog watchdog;

    void OnEnable()
    {
        Log.Debug($"{this} OnEnable");
        watchdog = GetComponent<PelvisWatchdog>();
        this.watchdog.Behavior = this;
        this.SetBaseVisibility(false);
        PlayerInstances.DefaultPlayer.NotifySpawned(watchdog);
    }

    void OnDisable()
    {
        Log.Debug("Restoring previous pelvis due to ActressWatchdog.OnDisable");
        PlayerInstances.DefaultPlayer?.RestorePrevious(watchdog);
    }

    public ICarolType Constructor(PelvisWatchdog watchdog)
    {
        this.watchdog = watchdog;
        this.watchdog.Behavior = this;
        return this;
    }

    public void SetBaseOutfit(SourceDescriptor outfit) { }

    public void SetAnimator(RuntimeAnimatorController rac) { }

    public void SetHeightOffset(float height) { }

    public void SetBaseVisibility(bool visibility)
    {
        watchdog.CompData.SetBaseVisibility(visibility);
        watchdog.BoneData.StandardBones[Constants.HeadBone]
            .GetComponentsInChildren<SkinnedMeshRenderer>()
            .ToList()
            .ForEach(x => x.gameObject.SetActive(visibility));
    }

    public void Dispose() => Destroy(this);
}
