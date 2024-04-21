using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Utils;

public static class TransformExtensions
{
    /// <summary>
    /// Recursively generates a list of all children of the input transform.
    /// </summary>
    /// <param AssetName="rootBone">The uppermost bone in the list.</param>
    /// <returns>A new List containing rootBone and all of it's descendants. </returns>
    public static List<Transform> SkeletonToList(this Transform rootBone)
    {
        //TODO: refactor this to create a dictionary instead, GetComponentsInChildren<Transform> seems fast enough
        List<Transform> outputList = new();
        if (rootBone is null) return outputList;
        outputList.Add(rootBone);
        SkeletonToList(rootBone, outputList);
        return outputList;
    }

    /// <summary>
    /// Recursively generates a list of all children of the input transform.
    /// </summary>
    /// <param AssetName="rootBone">The uppermost bone in the list.</param>
    /// <param AssetName="inputList">List of transforms to append</param>
    /// <returns>T</returns>
    private static List<Transform> SkeletonToList(this Transform rootBone, List<Transform> inputList)
    {
        foreach (Transform bone in rootBone)
        {
            inputList.Add(bone);
            SkeletonToList(bone, inputList);
        }
        return inputList;
    }

    /// <summary>
    /// Recursively finds a transform based on an input query. 
    /// </summary>
    /// <param AssetName="transform">The uppermost transform to include in the search.</param>
    /// <param AssetName="query">Conditional expression to search for.</param>
    /// <returns>The first transform found that returns true for the input query, or null.</returns>
    public static Transform RecursiveFindTransform(this Transform transform, Predicate<Transform> query)
    {
        if (!transform) return null;
        if (query(transform)) { return transform; }

        foreach (Transform child in transform)
        {
            var result = child.RecursiveFindTransform(query);
            if (result) return result;
        }
            
        return null;
    }

    public static List<Transform> RecursiveFindTransforms(this Transform transform, Predicate<Transform> query, ref List<Transform> existingList)
    {
        existingList ??= new();
        if (!transform) return null;
        if (query(transform)) { existingList.Add(transform); }

        foreach (Transform child in transform)
        {
            var result = child.RecursiveFindTransforms(query, ref existingList);
        }

        return existingList;
    }


    /// <summary>
    /// Removes the string "(Clone)" from the name of a transform.
    /// </summary>
    /// <param name="transform">The transform to be renamed.</param>
    public static void DeCloneName(this Transform transform)
    {
        transform.name = transform.name.Replace("(Clone)", "");
    }

    /// <summary>
    /// Searches parents for a component. This extension exists because Unity 2017 doesn't include inactive objects lol.
    /// </summary>
    /// <typeparam name="T">Component type to find.</typeparam>
    /// <param name="transform">Lowest child to search through.</param>
    /// <param name="includeInactive">Include inactive GameObjects in search.</param>
    /// <returns>The first found instance of the given type.</returns>
    public static T GetComponentInParent<T>(this Transform transform, bool includeInactive) where T : Component
    {
        if (transform == null) return null;
        if (!transform.gameObject.activeSelf && !includeInactive) return null;
        
        var component = transform.GetComponent<T>();
        if (component) return component;
        var parent = transform.parent;
        if (parent) return parent.GetComponentInParent<T>(includeInactive);

        return null;
    }

    /// <summary>
    /// Recurses through a Transform, instantiating the GameObject at the first Transform matching the given predicate.
    /// </summary>
    /// <param name="root">Topmost candidate Transform.</param>
    /// <param name="original">GameObject to instantiate</param>
    /// <param name="query">Predicate to search by.</param>
    /// <returns>The transform of the instantiated GameObject</returns>
    public static Transform InstantiateOnChild(this Transform root, GameObject original, Predicate<Transform> query)
    {
        if (!root || !original) return null;

        var parent = root.RecursiveFindTransform(query);
        if (!parent) return null;

        return GameObject.Instantiate(original, parent).transform;
    }

    /// <summary>
    /// Instantiates a copy of the transform on all list members that match the predicate. 
    /// </summary>
    /// <param name="original">Transform to instantiate.</param>
    /// <param name="potentialTargets">List of items to check.</param>
    /// <param name="query">Predicate that determines instantiation.</param>
    /// <returns>List of instantiated objects. </returns>
    public static List<Transform> InstantiateWhere(this Transform original, IEnumerable<Transform> potentialTargets, Predicate<Transform> query)
    {
        List<Transform> instantiated = new();
        if (potentialTargets is null) return instantiated;

        var targets = potentialTargets.Where(x=> query(x));

        foreach (var target in targets)
        {
            var created = GameObject.Instantiate(original.gameObject, target).transform;
            if (!created) continue;
            instantiated.Add(created);
        }

        return instantiated;
    }

    /// <summary>
    /// Reorders all children of a GameObject based on a provided comparison.
    /// </summary>
    /// <param name="parent">Parent of children to sort.</param>
    /// <param name="comparison"></param>
    public static void SortChildren(this Transform parent, Comparison<Transform> comparison)
    {
        var kids = parent.Cast<Transform>().ToList();
        kids.Sort(comparison);
        foreach (var child in kids)
        {
            child.transform.SetAsLastSibling();
        }
    }

    public static void ResetLocalPosRot(this Transform target)
    {
        target.transform.localRotation = Quaternion.identity;
        target.transform.localPosition = Vector3.zero;
    }
}
