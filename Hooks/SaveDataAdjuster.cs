using CarolCustomizer.Utils;
using System.Collections;
using UnityEngine;

namespace CarolCustomizer.Hooks;
public class SaveDataAdjuster
{
    public static IEnumerator EnsurePyjamas()
    {
        yield return new WaitUntil(() => SaveManager.manager?.data is not null);
        Log.Info("Setting Pyjamas in save file");
        foreach (var save in SaveManager.manager.data)
        {
            save.players[0].inventory.outfit = Constants.Pyjamas;
            save.players[0].inventory.accessory = 0;
        }
        Log.Info("Save file outfit overwritten.");
    }
}
