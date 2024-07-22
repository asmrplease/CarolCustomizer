using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Carol;
internal class PlayerInstances : IDisposable
{
    static List<PlayerCarolInstance> Players;
    public static IEnumerable<PlayerCarolInstance> ValidPlayers => Players.Where(x => x.Exists());
    public static PlayerCarolInstance DefaultPlayer => Players.First();
    static int numPlayers = 4;

    public PlayerInstances(Transform parent)
    {
        Players = Enumerable.Range(0, numPlayers)
            .Select(i => new PlayerCarolInstance(parent, i))
            .ToList();
    }

    public static void OnPlayerSpawn(PlayerWatchdog watchdog)
    {
        Log.Debug("OnPlayerSpawn()");
        if (!watchdog.carolEntity) { Log.Error("Watchdog spawned with null Entity"); return; }

        int index = PlayerIndex(watchdog.carolEntity);
        if (index < 0) { Log.Error("No matching player entity found "); return; }
        if (index >= Players.Count) { Log.Warning($"index {index} was outside of player count {Players.Count}"); return; }

        Log.Debug($"Players[{index}].NotifySpawned()");
        Players[index].NotifySpawned(watchdog);
    }

    //handle cutscene start?
    //handle cutscene end?

    public static int PlayerIndex(Entity entity) => 
        Entity.players is null ? -1:
        Entity.players.IndexOf(entity);

    public static PlayerCarolInstance Find(Entity entity) => Players.FirstOrDefault(x => x.ManagesPlayer(entity));

    public static bool IsPlayer(Entity entity) => Players.Any(x => x.ManagesPlayer(entity));

    public void Dispose()
    {
        Players
            .Where(x => x is not null)
            .ForEach(x => x.Dispose());
    }
}
