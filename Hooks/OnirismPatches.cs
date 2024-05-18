using HarmonyLib;
using UnityEngine;
using CarolCustomizer.Utils;
using System.Linq;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Assets;

namespace CarolCustomizer.Hooks;

public static class OnirismPatches
{
    [HarmonyPatch(typeof(Projectile), "OnTriggerEnter")]
    public class ProjectileCollision
    {
        [HarmonyPostfix]
        public static void PostFix(Collider collider, Projectile __instance)
        {
            if (!Entity.players.Contains(__instance.origin)) return;

            var uis = CCPlugin.uiInstances.Where(x => x.playerManager.ManagesPlayer(__instance.origin));
            if (uis.Count() != 1) { Log.Debug($"when firing a projectile, found {uis.Count()} players that matched."); return; }

            var ui = uis.First();
            ui.materialManager.OnNewTarget(collider.gameObject);
        }
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.LoadAssetsFunction))]
    public static class LoadAssetsReplacement
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            Log.Info("LoadAssetsPrefix");
            if (CCPlugin.gmRewrite) { CCPlugin.gmRewrite.LoadAssetsFunction(); return false; }
            return true;
        }
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
            Log.Debug($"CostumeSwapUI: {modelData.name}");

            RecipeApplier.ActivateFirstVariant(
                CCPlugin
                    .cutscenePlayer
                    .outfitManager,
                OutfitAssetManager
                    .GetOutfitByAssetName(modelData.name).AssetName);
            return false;
        }
    }

    [HarmonyPatch(typeof(CostumeSwapUI), nameof(CostumeSwapUI.ToggleAccessory))]
    public class AccessoryTogglePatch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            return false;
        } 
    }

    [HarmonyPatch(typeof(SwapCostumeTrigger), "OnTriggerEnter")]
    public class DisableSwapCostumeTrigger
    {
        [HarmonyPrefix]
        public static bool Prefix(Collider intrus)
        {
            var entity = intrus.GetComponent<Entity>();
            return !CCPlugin.playerManagers.Any(x => x.ManagesPlayer(entity));
        }
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.Load))]
    public static class SetPyjamasInSave
    {
        [HarmonyPostfix]
        public static void Postfix() => SaveDataAdjuster.SetPyjamas();
    }
}