using CarolCustomizer.Hooks.Watchdogs;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Onirism.Ui.ElixirDispenserPanel;

namespace CarolCustomizer.Utils;
public static class OnirismExtensions
{
    /// <summary>
    /// Finds the CarolPelvis by addressing the skeleton of the newTarget Entity.
    /// </summary>
    /// <returns>Null, or the CarolPelvis transform of the current newTarget entity.</returns>
    public static Transform GetPelvis(this Entity entity)
    {
        if (!entity) return null;
        return entity.currentModel?.spineBone?.transform.parent.parent.parent;
    }

    /// <summary>
    /// Removes common unity/onirism text from strings. 
    /// </summary>
    /// <param name="name">String to clean</param>
    /// <returns>String without CAROLMOD_, CAROL_,  '_', and (Clone).</returns>
    public static string CleanName(this string name)
    {
        return name.Replace("CAROLMOD_", "").Replace("CAROL_", "").Replace("(Clone)", "").Replace("_", " ").Trim();
    }

    public static MultiplayerManager.PlayerStats GetPlayerStats(this VirtualCarol virtualCarol)
    {
        if (MultiplayerManager.manager?.players is null) return null;
        if (!virtualCarol) return null;
        return MultiplayerManager.manager.players
            .FirstOrDefault(x => x.player == virtualCarol.entity);
    }

    public static GameObject GetMenuCarolPelvis()
    {
        return SceneManager
            .GetActiveScene()
            .GetRootGameObjects()
            .First(x => x.name == "MenuCarolLoader")
            .transform
            .RecursiveFindTransform(x => x.name == Constants.PelvisBone)
            .gameObject;
    }

    public static IEnumerator EnableSpacesuit(this PelvisWatchdog watchdog)
    {
        Log.Info("EnableSpacesuit()");
        watchdog.CompData.allSMRs
            .Where(x => x.name == "Spacehelmet")
            .ForEach(x => x.gameObject.SetActive(true));
        if (SceneManager.GetActiveScene().name != "Spacestation") yield break;

        watchdog.CompData.allSMRs
            .Where(x => x.name == "Spacesuitbackpack")
            .ForEach(x => x.gameObject.SetActive(true));
        yield break;
    }

    public static string GetParentBoneName(this Accessory acc)
    {
        return acc.slot switch
        {
            Accessory.AccessorySlot.Head => Constants.HeadBone,
            Accessory.AccessorySlot.Eyes => Constants.HeadBone,
            Accessory.AccessorySlot.Mouth => Constants.HeadBone,
            Accessory.AccessorySlot.HairClipLeft => Constants.HeadBone,
            Accessory.AccessorySlot.HairClipRight => Constants.HeadBone,
            Accessory.AccessorySlot.AnimalEars => Constants.HeadBone,
            Accessory.AccessorySlot.Ears => Constants.HeadBone,
            Accessory.AccessorySlot.Back => Constants.BackBone,
            Accessory.AccessorySlot.Chest => Constants.ChestBone,
            Accessory.AccessorySlot.BackPelvis => Constants.PelvisBone,
            Accessory.AccessorySlot.FrontPelvis => Constants.PelvisBone,
            Accessory.AccessorySlot.HipLeft => Constants.PelvisBone,
            Accessory.AccessorySlot.HipRight => Constants.PelvisBone,
            Accessory.AccessorySlot.LeftArm => Constants.LeftArmBone,
            Accessory.AccessorySlot.RightArm => Constants.RightArmBone,
            Accessory.AccessorySlot.LeftLeg => Constants.LeftLegBone,
            Accessory.AccessorySlot.RightLeg => Constants.RightLegBone,
            _ => Constants.PelvisBone,
        };
    }
}
