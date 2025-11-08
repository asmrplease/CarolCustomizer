using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Models;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Utils;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static System.Linq.Enumerable;

namespace CarolCustomizer.Behaviors;
internal static class AccessoryDissolver
{
    const string DissolveAmount = "_DissolveAmount";
    const string FireDissolveAddress = "Materials/Deathdissolvefire";
    
    static Coroutine ActiveDissolve;
    static bool FadeFinish = false;
    static Material FireDissolve;

    static AccessoryDissolver()
    {
        SceneManager.activeSceneChanged += HandleSceneChanged;
        FireDissolve = Resources.Load<Material>(FireDissolveAddress);
    }

    public static void Dispose()
    {
        SceneManager.activeSceneChanged -= HandleSceneChanged;
    }

    static void HandleSceneChanged(Scene arg0, Scene arg1)
    {
        Log.Debug($"ActiveSceneChanged {arg0.name}, {arg1.name}");
        FadeFinish = true;
    }

    [HarmonyPatch(typeof(CheckPoints), "OnTriggerEnter")]
    class CheckPointEnterPatch
    {
        [HarmonyPostfix]
        static void Postfix()
        {
            FadeFinish = true;
        }
    }

    [HarmonyPatch(typeof(SceneSwitcher), "OnTriggerStay")]
    class SceneSwitchPatch
    {
        [HarmonyPostfix] 
        static void Postfix(Collider other, SceneSwitcher __instance)
        {
            if (!__instance.dissolves) return;
            if (ActiveDissolve is not null) return;
            
            Entity playerEntity = other.GetComponent<Entity>();
            if (!playerEntity) return;

            var player = PlayerInstances.Find(playerEntity);
            if (player is null) return;

            ActiveDissolve = CCPlugin
                .CoroutineRunner
                .StartCoroutine(
                    Dissolve(
                        player.outfitManager, 
                        __instance.dissolveMaterial, 
                        __instance.dissolveTime));
        }
    }

    [HarmonyPatch(typeof(Entity), "Die")]
    class DeathPatch
    {
        [HarmonyPostfix]
        static void Postfix(Entity.DeathType deathType, Entity __instance)
        {
            Log.Debug("Die postfix!");
            if (!(deathType == Entity.DeathType.Fire || deathType == Entity.DeathType.InstantFire)) return;
            if (__instance.healthOptions.downed.isDowned) return;
            if (GameManager.manager.coop.isCoop) return;

            var player = PlayerInstances.Find(__instance);
            if (player is null) return;

            ActiveDissolve = CCPlugin
                .CoroutineRunner
                .StartCoroutine(
                    Dissolve(
                        player.outfitManager, 
                        FireDissolve, 
                        0.25f));
        }
    }

    //TODO: we need to handle the 'downed' state better- making the player invisible when they're downed probably isn't how the game was meant to be played
    //TODO: either get the hair to act like another accessory, or create a SetHairColorShared that can apply the dissolve correctly

    public static IEnumerator Dissolve(OutfitCoordinator outfitManager, Material dissolveMaterial, float time)
    {
        Log.Debug("Dissolve()");
        float elapsedTime = 0f;
        FadeFinish = false;
        var accs = outfitManager.ActiveAccessories;
        var liveDescriptions = outfitManager.LiveAccessoryDescriptors;
        //var hairColor = outfitManager.HairMaterialName;
        var dissolveMat = new MaterialDescriptor(dissolveMaterial, "Resources", SourceType.Resources);
        Dictionary<AccessoryDescriptor, MaterialDescriptor[]> originalMaterials = [];

        foreach (var acc in accs)
        {
            originalMaterials[acc] = liveDescriptions
                .FirstOrDefault(x => x.Name == acc.Name && x.Source == acc.Source)
                .Materials;
            outfitManager.PaintAccessoryShared(acc, acc.Materials.Select(x => dissolveMaterial).ToList());
        }
        //outfitManager.SetHairColor(dissolveMaterial, true);
        Log.Debug("Accessories Painted with dissolve, starting loop.");

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float dissolvePercent = (elapsedTime / time);
            dissolveMaterial.SetFloat(DissolveAmount, dissolvePercent);
            yield return null;
        }
        Log.Debug("Done dissolving, waiting for scene exit or player respawn");

        yield return new WaitUntil(() => FadeFinish);

        foreach (var acc in accs)
        {
            foreach (int i in Range(0, acc.Materials.Length))
            {
                outfitManager.PaintAccessory(acc, originalMaterials[acc][i], i);
            }
        }
        //outfitManager.SetHairColor(OutfitAssetManager.GetHairColorMaterial(hairColor));
        Log.Debug("Restored original materials");
        ActiveDissolve = null;
        yield break;
    }
}
