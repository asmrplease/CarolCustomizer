using HarmonyLib;
using UnityEngine;
using CarolCustomizer.Utils;
using System.Linq;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Assets;
using MagicaCloth2;
using System.ComponentModel;

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

    [HarmonyPatch(typeof(MagicaCloth), nameof(MagicaCloth.BuildAndRun))] 
    public static class DebugInit
    {
        [HarmonyPostfix]
        public static void Postfix() => Log.Info("MagicaCloth.BuildAndRun Postfix");
    }

    [HarmonyPatch(typeof(ClothProcess), "StartRuntimeBuild")]
    public static class DebugInit2
    {
        [HarmonyPrefix]
        public static void Postfix(ClothProcess __instance) 
        { 
            Log.Info("ClothProcess.StartRuntimeBuild");
            Log.Debug($"Process Type: {__instance.cloth.SerializeData.clothType}");
            string smrName = __instance.cloth.SerializeData.sourceRenderers.FirstOrDefault()?.name ?? "null";
            Log.Debug($"TargetSMR: {smrName}");
        }
    }
}