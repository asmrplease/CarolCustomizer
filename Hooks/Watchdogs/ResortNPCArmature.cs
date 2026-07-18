using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using Onirism;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs;
internal class ResortNPCArmature : MonoBehaviour, ICarolType, ICustomizable
{
    PelvisWatchdog watchdog;

    void Awake()
    {
        watchdog = PelvisWatchdog.GetAddWatchdog(gameObject);
        if (!watchdog) { Log.Error("Failed to get watchdog during ResortNPCArmature.Awake()"); return; }

        watchdog.Behavior = this;

        NPCManager.OnBotSpawn(this);
    }

    public void Customize(RecipeDescriptor recipe, OutfitCoordinator outfitManager, string name)
    {
        SetBaseVisibility(false);
        RecipeApplier.ActivateRecipe(outfitManager, recipe);
    }

    public PelvisWatchdog Watchdog() => watchdog;

    public ICarolType Constructor(PelvisWatchdog watchdog)
    {
        //throw new System.NotImplementedException();
        return this;
    }

    public void SetBaseOutfit(SourceDescriptor outfit)
    {
        //throw new System.NotImplementedException();
    }

    public void SetAnimator(RuntimeAnimatorController rac)
    {
        //throw new System.NotImplementedException();
    }

    public void SetHeightOffset(float height)
    {
        //throw new System.NotImplementedException();
    }

    public void SetBaseVisibility(bool visibility) => StartCoroutine(BaseVisibility(visibility));

    IEnumerator BaseVisibility(bool visibility)
    {
        yield return new WaitForSeconds(3);

        watchdog
            .transform
            .parent
            .GetComponents<ChildRandomizer>()
            .SelectMany(rand => rand.children)
            .ForEach(t => t.gameObject.SetActive(visibility));
    }

    public void Dispose()
    {
        //throw new System.NotImplementedException();
    }
}

