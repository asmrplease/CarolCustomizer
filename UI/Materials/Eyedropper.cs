using CarolCustomizer.Assets;
using CarolCustomizer.Models;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Models.Outfits;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace CarolCustomizer.UI.Materials;
internal class Eyedropper : MonoBehaviour
{
    GameObject lastHit;
    readonly float scanInterval = 0.10f;
    readonly Vector2 hotspot = new(4, 28);
    Texture2D cursor;
    float nextScan = 0;
    internal event Action<List<MaterialDescriptor>> OnMaterialsFound;

    internal Eyedropper Constructor(UIAssetLoader uiAssets)
    {
        this.cursor = uiAssets.CursorTexture;
        return this;
    }

    void OnEnable()
    {
        Cursor.SetCursor(this.cursor, hotspot, CursorMode.Auto);
    }

    void OnDisable()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    void Update()
    {
        if (Time.time < nextScan) return;

        nextScan = Time.time + scanInterval;
        if (EventSystem.current.IsPointerOverGameObject()) return; //don't change the list if the pointer is over a UI element

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out RaycastHit cast, 1000f);
        if (!cast.transform) return;

        var hit = cast.transform.gameObject;
        if (lastHit && hit == lastHit) return;
        if (hit.name == "CAROL(Clone)") return;

        lastHit = hit;
        var scene = SceneManager.GetActiveScene().name;
        var results = hit
            .GetComponentsInChildren<Renderer>(true)
            .Concat(hit.GetComponentsInParent<Renderer>(true))
            .SelectMany(x => x.materials)
            .Distinct()
            .Select(x => new MaterialDescriptor(x, new SourceDescriptor(scene, SourceType.World)))
            .ToList();
        if (results.Any()) OnMaterialsFound?.Invoke(results);
    }
    
}
