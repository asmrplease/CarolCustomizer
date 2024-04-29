using System.Linq;
using System.Reflection;
using UnityEngine;

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

    /// <summary>
    /// Updates dynbone components to support adding arbitrary bones, requires reflection calls. 
    /// </summary>
    /// <param name="dBone">Dynamic Bone to restart.</param>
    public static void RestartDynamicBone(this DynamicBone dBone)
    {
        //TODO: can we cache these MethodInfo objects to make this faster?
        if (dBone == null) { Log.Warning("Couldn't find hair DynamicBone!"); return; }//TODO: try to instantiate if null? for now just don't NRE on the next line
        typeof(DynamicBone).GetMethod("InitTransforms", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(dBone, null);
        typeof(DynamicBone).GetMethod("SetupParticles", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(dBone, null);
    }

    public static MultiplayerManager.PlayerStats GetPlayerStats(this VirtualCarol virtualCarol)
    {
        if (MultiplayerManager.manager?.players is null) return null;
        if (!virtualCarol) return null;
        //Log.Debug($"Getting Player: {virtualCarol.name} from:");
        //foreach (var mp in MultiplayerManager.manager.players) { Log.Debug(mp.name); } 
        return MultiplayerManager.manager.players
            .FirstOrDefault(x => x.player == virtualCarol.entity);
    }
}
