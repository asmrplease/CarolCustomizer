using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CarolCustomizer.Models;
using CarolCustomizer.Utils;
using UnityEngine.SceneManagement;

namespace CarolCustomizer.Behaviors;
public class MaterialManager
{
    public MaterialDescriptor clipboard;

    GameObject currentTarget;
    
    public List<MaterialDescriptor> currentMaterials { get; private set; }

    public MaterialManager()
    {
        
    }

    public void OnNewTarget(GameObject newObject)
    {
        if (!newObject) return;
        this.currentTarget = newObject;
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
        currentMaterials = materials.Select(x=> new MaterialDescriptor(x, sceneName, MaterialDescriptor.SourceType.World )).ToList();
        Log.Debug($"Current material count: {currentMaterials.Count()}");
        return currentMaterials;
    }

    public MaterialDescriptor GetFirstWorldMaterial()
    {
        ListMaterials();
        if (currentMaterials is null) { Log.Debug("no materials exist on current object"); return null; }

        return currentMaterials[0];
    }
}
