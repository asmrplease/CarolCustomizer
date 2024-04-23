using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CarolCustomizer.Models;
using CarolCustomizer.Utils;
using UnityEngine.SceneManagement;

namespace CarolCustomizer.Behaviors.Carol;
public class MaterialManager
{
    public MaterialDescriptor clipboard;

    GameObject currentTarget;

    public List<MaterialDescriptor> currentMaterials { get; private set; }

    public void OnNewTarget(GameObject newObject)
    {
        if (!newObject) return;
        currentTarget = newObject;
    }

    public List<MaterialDescriptor> ListMaterials()
    {
        Log.Debug("ListMaterials()");
        if (!currentTarget) { Log.Warning("current target is null."); return null; }

        var renderers = currentTarget.GetComponentsInChildren<Renderer>(true);

        if (renderers.Count() == 0) { Log.Warning("no smrs were found in target."); return null; }

        List<Material> materials = new();
        foreach (var renderer in renderers)
        {
            materials.AddRange(renderer.materials);
        }

        string sceneName = SceneManager.GetActiveScene().name;

        Log.Debug($"Found {materials.Count} materials in {currentTarget.name}.");
        currentMaterials = materials.Select(x => new MaterialDescriptor(x, sceneName, MaterialDescriptor.SourceType.World)).ToList();
        Log.Debug($"Current material count: {currentMaterials.Count()}");
        return currentMaterials;
    }

    public MaterialDescriptor GetFirstWorldMaterial()
    {
        ListMaterials();
        if (currentMaterials is null) { Log.Debug("no materials exist on current object"); return null; }

        return currentMaterials[0];
    }

    public static void FindMaterials()
    {
        var materials = Resources.FindObjectsOfTypeAll<Material>();
        foreach (var material in materials) { Log.Debug(material.name); }
    }
}
