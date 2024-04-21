using HarmonyLib;
using UnityEngine;
using CarolCustomizer.Utils;
using System.Linq;

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
    public class LoadAssetsReplacement
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            Log.Info("LoadAssetsPrefix");
            if (CCPlugin.gmRewrite) { CCPlugin.gmRewrite.LoadAssetsFunction(); return false; }
            return true;
        }
    }
}