using CarolCustomizer.Assets;
using CarolCustomizer.Contracts;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Carol;

public class EffectManager : IPelvisFollower
{
    PelvisWatchdog pelvis;
    SkeletonManager skeletonManager;

    HashSet<Outfit> outfitEffects = [];
    Outfit animatorSource;
    HaDSOutfit configurationSource;

    public string AnimatorSource => animatorSource?.AssetName ?? Constants.Pyjamas;
    public string ConfigurationSource => configurationSource?.AssetName ?? Constants.Pyjamas;
    public IEnumerable<string> ActiveEffects =>
        outfitEffects
        .Select(x => x.AssetName);

    public EffectManager(SkeletonManager skeletonManager)
    {
        this.skeletonManager = skeletonManager;
        animatorSource
            = configurationSource
            = OutfitAssetManager.GetPyjamas();
    }

    public void HandleNewPelvis(PelvisWatchdog watchdog)
    {
        this.pelvis = watchdog;
        if (outfitEffects.Any()) RefreshEffects();
        if (animatorSource is not null) SetAnimator(animatorSource);
        if (configurationSource is not null) ApplyConfig();
    }

    void RefreshEffects()
    {
        outfitEffects
            .ForEach(x =>
               SetEffect(x, true));
    }

    public void DisableAllEffects()
    {
        outfitEffects
            .ToList()
            .ForEach(x => SetEffect(x, false));
    }

    public void SetEffect(Outfit outfit, bool enabled)
    {
        if (!pelvis || pelvis.Behavior is null || outfit is null) return;
        if (!outfit.Effects.Any()) return;

        skeletonManager.GetAddBoneSet(outfit.AssetName, outfit.boneData.BespokeBones);
        foreach (var effect in outfit.Effects)
        {
            var transform = pelvis.transform.Find(effect.RelativePath);
            if (!transform) continue;

            switch (effect.Type)
            {
                case OutfitEffect.ComponentType.Behavior:
                    transform
                        .GetComponents<Behaviour>()
                        .ForEach(x => x.enabled = enabled);
                    break;
                case OutfitEffect.ComponentType.Component:
                    transform
                        .gameObject
                        .SetActive(enabled);
                    break;
            }
        }

        if (enabled) outfitEffects.Add(outfit);
        if (!enabled) outfitEffects.Remove(outfit);
    }

    void ApplyConfig()
    {
        if (!pelvis || pelvis.Behavior is null) return;
        if (configurationSource is null) return;

        Log.Debug("ApplyConfig()");
        pelvis.Behavior.SetHeightOffset(configurationSource.modelData.height);
    }

    public void SetConfiguration(HaDSOutfit outfit)
    {
        Log.Debug("SetConfiguration()");
        if (outfit is null) { Log.Warning("outfit is null"); return; }

        this.configurationSource = outfit;
        ApplyConfig();
    }

    public void SetAnimator(Outfit outfit)
    {
        if (!pelvis || pelvis.Behavior is null) { Log.Warning("Tried to swap animators with no pelviswatchdog instantiated."); return; }
        if (outfit is null) { Log.Warning("Tried to load animator from null outfit"); return; }

        Log.Debug($"changing animator to {outfit.DisplayName}");
        pelvis.Behavior.SetAnimator(outfit);
        this.animatorSource = outfit;
    }
}
