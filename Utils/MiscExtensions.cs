using BepInEx.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Button = UnityEngine.UI.Button;

namespace CarolCustomizer.Utils;
/// <summary>
/// Extensions that are specific to Onirism
/// </summary>
public static class MiscExtensions
{
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

    public static Type[] AllAssetsAs<Type>(this AssetBundleRequest request) where Type : class
    {
        var assets = new Type[request.allAssets.Length];

        int i = 0; foreach (var asset in request.allAssets)
        {
            assets[i++] = asset as Type;
        }

        return assets;
    }

    public static Dictionary<TKey,TValue> ToDictionaryOverwrite<TKey, TValue>(this IEnumerable<TValue> enumerable, Func<TValue, TKey> keySelector)
    {
        var results = new Dictionary<TKey, TValue>();
        foreach (var item in enumerable)
        {
            results[keySelector(item)] = item;
        }
        return results;
    }

    public static Dictionary<TKey, TValue> ToDictionaryRename<TKey, TValue>(
        this IEnumerable<TValue> enumerator, 
        Func<TValue, TKey> keySelector, 
        Action<TValue> rename,
        int retryCount = 5)
        where TValue : class
    {
        var results = new Dictionary<TKey, TValue>();
        foreach (var item in enumerator)
        {
            var temp = item;
            foreach (int i in Enumerable.Range(0, retryCount+1))
            {
                var key = keySelector(temp);
                results.TryGetValue(key, out var existing);
                if (existing is null) { results[key] = temp; break; }
                if (existing == temp) { break; }
                if (i != retryCount) rename(temp);
                else { Log.Error($"failed to rekey"); break; }
            }
        }
        return results;
    }

    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        foreach (T item in sequence) action(item);
        return sequence;
    }

    public static ConfigEntry<T> AsConfigEntry<T>(this EventArgs e)
    {
        var idk = e as SettingChangedEventArgs;
        var wtf = idk.ChangedSetting as ConfigEntry<T>;
        return wtf;
    }

    public static Button SetupButton(this Transform transform, string address, UnityAction callback)
    {
        var button = transform
            .Find(address)
            .GetComponent<Button>();
        button.onClick.AddListener(callback);
        return button;
    }
}
