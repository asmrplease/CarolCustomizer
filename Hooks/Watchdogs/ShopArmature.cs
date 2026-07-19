using CarolCustomizer.Assets;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs.UnhandledArmatures;
internal class ShopArmature : MonoBehaviour, ICarolType
{
    static List<Entity> lastCheck = [];

    PelvisWatchdog watchdog;
    int playerIndex = -1;

    public ICarolType Constructor(PelvisWatchdog watchdog)
    {
        this.watchdog = watchdog;
        var all = GetEntitiesInShops();
        var newPlayers = all.Except(lastCheck);
        lastCheck = all;

        if (newPlayers.Count() < 1) { Log.Warning($"No new players were detected in a shop when this ShopArmature was constructed."); return this; }
        if (newPlayers.Count() > 1) { Log.Warning($"Multiple new players entered a shop on this frame, count: {newPlayers.Count()}"); return this; }

        var player = newPlayers
            .First()
            .GetComponentInChildren<PlayerArmature>(true);
        if (player is null) { Log.Warning("No player armature detected on entity entering shop."); return this; }

        playerIndex = PlayerInstances.PlayerIndex(player.carolEntity);
        PlayerInstances
            .Players
            .ElementAt(playerIndex)
            .NotifySpawned(watchdog);
        return this;
    }

    static List<Entity> GetEntitiesInShops()
    {
        var shopPlayers = ShopStand
            .shops
            .SelectMany(shop => shop.playersInShop);
        var toyboxPlayers = ToyBoxStand
            .shops
            .SelectMany(box => box.playersInShop);
        return shopPlayers
            .Concat(toyboxPlayers)
            .ToList();
    }

    //when deactivated,
    //notify the player instance we found at the start
    //that this armature has deactivated
    void OnDisable() => Dispose();

    public void Dispose()
    {
        lastCheck = GetEntitiesInShops();
        if (playerIndex < 0) return;

        PlayerInstances
            .Players
            .ElementAt(playerIndex)
            .RestorePrevious(watchdog);
    }

    public void SetAnimator(RuntimeAnimatorController rac) { }
    public void SetBaseOutfit(SourceDescriptor outfit) { }
    public void SetBaseVisibility(bool visibility) { }
    public void SetHeightOffset(float height) { }
}
