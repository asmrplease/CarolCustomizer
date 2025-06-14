using HarmonyLib;
using UnityEngine;
using CarolCustomizer.Utils;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Settings;

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

            CCPlugin.uiInstance.materialManager.OnNewTarget(collider.gameObject);
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

            var player = PlayerInstances.Find(__instance.player);
            if (player is null) return false;

            Log.Debug($"CostumeSwapUI: {modelData.name}");
            RecipeApplier.ActivateFirstVariant(
                player.outfitManager,
                OutfitAssetManager.GetOutfitByAssetName(modelData.name).AssetName);
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
}