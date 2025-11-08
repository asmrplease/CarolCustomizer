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

    HashSet<SourceDescriptor> outfitEffects = [];
    IAccessorySource animatorSource;
    IAccessorySource configurationSource;

    public SourceDescriptor AnimatorSource => animatorSource.Descriptor ?? Constants.PyjamaDescriptor;
    public SourceDescriptor ConfigurationSource => configurationSource.Descriptor ?? Constants.PyjamaDescriptor;
    public IEnumerable<SourceDescriptor> ActiveEffects => outfitEffects;

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
        if (animatorSource is not null) SetAnimator(animatorSource.Descriptor);
        if (configurationSource is not null) SetConfiguration(configurationSource.Descriptor);
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

    public void SetEffect(SourceDescriptor desc, bool enabled)
    {
        var source = OutfitAssetManager.GetAccessorySource(desc);
        if (!pelvis || pelvis.Behavior is null || source is null) return;
        if (!source.GetEffects().Any()) return;
        
        skeletonManager.GetAddBoneSet(desc, source.GetBespokeBones());
        foreach (var effect in source.GetEffects())
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

        if (enabled) outfitEffects.Add(desc);
        if (!enabled) outfitEffects.Remove(desc);
    }

    public void SetConfiguration(SourceDescriptor source)
    {
        Log.Debug("SetConfiguration()");
        if (source is null) { Log.Warning("outfit is null"); return; }

        if (source != configurationSource.Descriptor) { configurationSource = OutfitAssetManager.GetAccessorySource(source); }
        pelvis.Behavior.SetHeightOffset(configurationSource.GetConfiguration().height);
    }

    public void SetAnimator(SourceDescriptor source)
    {
        if (!pelvis || pelvis.Behavior is null) { Log.Warning("Tried to swap animators with no pelviswatchdog instantiated."); return; }
        if (source is null) { Log.Warning("Tried to load animator from null outfit"); return; }

        if (source != animatorSource.Descriptor) { animatorSource = OutfitAssetManager.GetAccessorySource(source); }
        pelvis.Behavior.SetAnimator(animatorSource.GetAnimator());
    }
}
