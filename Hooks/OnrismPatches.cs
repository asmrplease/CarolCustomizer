using HarmonyLib;
using UnityEngine;
using CarolCustomizer.Utils;
using System.Linq;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Assets;

namespace CarolCustomizer.Hooks;

public static class OnrismPatches
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
            int variant = SaveManager
                .manager.data[SaveManager.manager.saveSlotCurrent]
                .players[0].inventory.accessory;//TODO: this line will fail in coop

            RecipeApplier.ActivateVariant(
                CCPlugin.cutscenePlayer.outfitManager,
                OutfitAssetManager.GetOutfitByAssetName(modelData.name),
                modelData.accessories[variant].name);
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

}