using HarmonyLib;
using UnityEngine;
using CarolCustomizer.Utils;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Hooks.Watchdogs;
using System.Linq;
using Onirism.Ui;
using UnityEngine.SceneManagement;

namespace CarolCustomizer.Hooks;

public static class OnirismPatches
{
    public static void FixTentCutscene(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Popcicle_peaks_Polarsea") return;

        Log.Info("Applying tent cutscene fix");
        Resources.FindObjectsOfTypeAll<Transform>()
            .Select(x => x.gameObject)
            .Where(x => x.name == "InsideTent")
            .ToList()
            .ForEach(x => x.SetActive(false));
    }

    private static void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        throw new System.NotImplementedException();
    }

    [HarmonyPatch(typeof(CostumeSwapUI), nameof(CostumeSwapUI.ChangeCostume))]
    public static class CostumeSwapPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(CostumeSwapUI __instance)
        {
            __instance.cardSlotToItem.TryGetValue(
                __instance.selection.transform.parent.gameObject,
                out var modelData);
            if (!modelData) return false;

            var player = PlayerInstances.Find(__instance.player);
            if (player is null) return false;

            Log.Debug($"CostumeSwapUI: {modelData.name}");
            var recipe = OutfitAssetManager.GetOutfitByAssetName(modelData.name)?.Variants.FirstOrDefault().Value;
            if (recipe is null) { Log.Error($"Failed to find any recipes in outfit {modelData.name}"); return false; }
            RecipeApplier.ActivateRecipe(
                player.outfitManager,
                recipe);
            return false;
        }
    }

    [HarmonyPatch(typeof(CostumeSwapUI), nameof(CostumeSwapUI.ToggleAccessory))]
    public class AccessoryTogglePatch
    {
        [HarmonyPrefix]
        public static bool Prefix() => false;
    }

    [HarmonyPatch(typeof(SwapCostumeTrigger), "OnTriggerEnter")]
    public class DisableSwapCostumeTrigger
    {
        [HarmonyPrefix]
        public static bool Prefix(Collider intrus)
        {
            var entity = intrus.GetComponent<Entity>();
            return !PlayerInstances.IsPlayer(entity);
        }
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.Load))]
    public static class SetPyjamasInSave
    {
        [HarmonyPostfix]
        public static void Postfix() => SaveDataAdjuster.SetPyjamas();
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LoadSave))]
    public static class SetPyjamasInVersus
    {
        [HarmonyPostfix]
        public static void Postfix(MainMenuManager __instance)
        {
            if (__instance.mapSelectType != "Versus") return;
            if (!Settings.Plugin.customMPBots.Value) return;

            MultiplayerSelection
                .selected
                .ForEach(x => 
                    x.skinPlaceholder = GameManager.manager.carolModel);
        }
    }

    [HarmonyPatch(typeof(ZombieRandomizer), "Start")]
    public static class SlimeCustomizationPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ZombieRandomizer __instance)
        {
            Log.Debug("ZombieRandomizer.Start().Prefix");
            if (__instance.GetComponentInChildren<SummerSlimeArmature>() is not SummerSlimeArmature slime) return true;

            Log.Debug("ZombieRandomizer detected SummerSlime component.");
            if (slime.custom is not true) return true;

            __instance.enabled = false;
            slime.SetBaseVisibility(false);
            return false;
        }
    }
}