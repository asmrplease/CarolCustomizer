using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CarolCustomizer.Utils;
/// <summary>
/// Extensions that are specific to Onirism
/// </summary>
public static class MiscExtensions
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

    public static string DeClone(this string name)
    {
        return name.Replace("(Clone)", "");
    }

    public static string DeInstance(this string name)
    {
        return name.Replace("(Instance)", "").Trim();
    }

    /// <summary>
    /// Iterates over a mesh's blendshape count to create an array of all blendshape weights.
    /// </summary>
    /// <param name="smr">Renderer to read from.</param>
    /// <returns>Array representing the current state of all weights.</returns>
    public static float[] GetAllBlendshapes(this SkinnedMeshRenderer smr)
    {
        if (!smr || !smr.sharedMesh) { Log.Error("Tried to get blendshapes from a null smr"); return null; }

        int count = smr.sharedMesh.blendShapeCount;
        float[] output = new float[count];

        for (int i = 0; i < count; i++)
        {
            output[i] = smr.GetBlendShapeWeight(i);
        }

        return output;
    }

    /// <summary>
    /// Applies an array of floats to the blendshape weights of the target SMR.
    /// </summary>
    /// <param name="smr">Renderer to apply weights to.</param>
    /// <param name="shapes">Values to apply.</param>
    public static void SetAllBlendshapes(this SkinnedMeshRenderer smr, float[] shapes)
    {
        if (!smr) { Log.Error("Tried to set blendshapes from a null smr"); return; }

        int ourCount = smr.sharedMesh.blendShapeCount;
        int theirCount = shapes.Length;

        int count = ourCount < theirCount ? ourCount : theirCount;
        //if (count != shapes.Length) { Log.Error("Tried to set blendshapes but array length did not match"); return; }

        for (int i = 0; i < count; i++)
        {
            smr.SetBlendShapeWeight(i, shapes[i]);
        }
    }

    /// <summary>
    /// Updates dynbone components to support adding arbitrary bones, requires reflection calls. 
    /// </summary>
    /// <param name="dBone">Dynamic Bone to restart.</param>
    public static void RestartHairJiggle(this DynamicBone dBone)
    {
        //TODO: can we cache these MethodInfo objects to make this faster?
        if (dBone == null) { Log.Warning("Couldn't find hair DynamicBone!"); return; }//TODO: try to instantiate if null? for now just don't NRE on the next line
        typeof(DynamicBone).GetMethod("InitTransforms", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(dBone, null);
        typeof(DynamicBone).GetMethod("SetupParticles", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(dBone, null);
    }

    /// <summary>
    /// Automatically calls IDisposable.Dispose() on any fields in the object
    /// </summary>
    public static void DisposeFields(this object target)
    {
        Log.Debug("reflective dispose");
        var fields = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        var disposables = fields.Where(x => typeof(IDisposable).IsAssignableFrom(x.FieldType));

        foreach (var field in disposables)
        {
            Log.Debug($"Disposing: {field.Name}:");
            var disposable = (IDisposable)field.GetValue(target);
            if (disposable is not null) disposable.Dispose();
        }
    }

    public static void ReplaceMaterialAtIndex(this SkinnedMeshRenderer smr, Material material, int index)
    {
        Log.Debug($"Setting {smr.name}[{index}] material to {material.name}");
        if (index < 0 || index > smr.materials.Length)
        { Log.Warning("Invalid material index"); return; }
        //When replacing a material, the whole array needs to be replaced at once
        var liveArray = smr.materials;
        liveArray[index] = material;
        smr.sharedMaterials = liveArray;
    }
    /// <summary>
    /// Inverts the value of a given boolean.
    /// </summary>
    /// <param name="value">The boolean to flip.</param>
    public static bool Flip(ref this bool value)
    {
        value = !value;
        return value;
    }

    public static IEnumerator SetTriggerForOneFrame(this Animator anim, string triggerName)
    {
        anim.SetBool(triggerName, true);
        yield return new WaitForEndOfFrame();
        anim.SetBool(triggerName, false);
        yield break;
    }

    public static MultiplayerManager.PlayerStats GetPlayerStats(this VirtualCarol virtualCarol)
    {
        if (!MultiplayerManager.manager) return null;
        return MultiplayerManager.manager.players.First(x=>x.player == virtualCarol.entity);
    }

    public static void UpdateBoneArray2(this SkinnedMeshRenderer target, SkinnedMeshRenderer source)
    {
        Transform pelvisParent;
        if (!target.rootBone)
        {
            pelvisParent = target.transform.RecursiveFindTransform(x => x.name == "CarolPelvis").parent;
        }
        else pelvisParent = target.transform.parent;

        if (!pelvisParent) { Log.Warning($"Failed to update bone array for {target.name}"); }

        var targetBoneList = pelvisParent.SkeletonToList().ToDictionaryOverwrite(x => x.name);

        Transform[] newBones = new Transform[source.bones.Length];
        int index = 0;
        foreach (var bone in source.bones)
        {
            Transform result;
            targetBoneList.TryGetValue(bone.name, out result);
            if (!result) { Log.Error($"Missing bone on {target.name}");}
            newBones[index++] = result;
        }
        target.bones = newBones;
    }

    public static Type[] AllAssetsAs<Type>(this AssetBundleRequest request) where Type : class
    {
        var assets = new Type[request.allAssets.Length];

        int i = 0; foreach (var asset in request.allAssets)
        {
            assets[i++] = asset as Type;
        }

        return assets;
    }

    public static Dictionary<TKey,TValue> ToDictionaryOverwrite<TKey, TValue>(this IEnumerable<TValue> enumerator, Func<TValue, TKey> keySelector)
    {
        var results = new Dictionary<TKey, TValue>();
        foreach (var item in enumerator)
        {
            results[keySelector(item)] = item;
        }
        return results;
    }
}
