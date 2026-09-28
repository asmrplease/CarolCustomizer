using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Reflection;

namespace CarolCustomizer.Hooks;
public class SaveDataAdjuster
{
    public static void SetPyjamas()
    {
        Log.Info("Setting Pyjamas in save file");
        if (SaveManager.manager.allSaves is null) return;

        foreach (var save in SaveManager.manager.allSaves)
        {
            var players = (List<SaveData.PlayerData>) typeof(SaveData).GetField("players", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(save);
            if (players is null) continue;

            players[0].inventory.outfit = Constants.Pyjamas;
            players[0].inventory.outfitSaved = Constants.Pyjamas;
            players[0].inventory.accessory = 0;
        }
        Log.Info("Save file outfit overwritten.");
    }
}
