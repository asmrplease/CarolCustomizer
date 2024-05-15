using CarolCustomizer.Utils;

namespace CarolCustomizer.Hooks;
public class SaveDataAdjuster
{
    public static void SetPyjamas()
    {
        Log.Info("Setting Pyjamas in save file");
        foreach (var save in SaveManager.manager.data)
        {
            save.players[0].inventory.outfit = Constants.Pyjamas;
            save.players[0].inventory.outfitSaved = Constants.Pyjamas;
            save.players[0].inventory.accessory = 0;
        }
        Log.Info("Save file outfit overwritten.");
    }
}
