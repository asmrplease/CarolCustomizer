﻿using CarolCustomizer;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Utils;
using Newtonsoft.Json;
using System.IO;
using System.Collections;
using UnityEngine;
using System.Linq;

namespace FaceCam.Behaviors;
public class ThumbnailCamera : MonoBehaviour
{
    readonly Vector3 cameraRotationAxis = new(0, 1, 0);
    Camera camera;
    CameraController cameraController;
    Transform cameraPostionDriver;
    Transform cameraTarget;
    int TextureSize = 512;

    void Awake()
    {
        Log.Debug("InsetCamera.Awake()");
        camera = this.GetComponent<Camera>();
        if (!camera) { Log.Error("InsetCamera instantiated on an object without a camera component!"); return; }

        this.name = "Inset Camera";
        var collider = GetComponent<SphereCollider>();
        if (collider) GameObject.Destroy(collider);

        camera.cullingMask = 1 << Constants.SMRLayer;
        camera.clearFlags = CameraClearFlags.Color; //CameraClearFlags.Color;//
        camera.backgroundColor = Constants.DefaultColor;
        camera.depth = Camera.main.depth + 1;
        camera.fieldOfView = 20;
        camera.aspect = 1;
        camera.useOcclusionCulling = false;
        camera.enabled = false;

        CCPlugin.cutscenePlayer.SpawnEvent += HandleNewPelvis;
        Slate.Cutscene.OnCutsceneStopped += HandleCutsceneEnd;

        cameraController = this.GetComponent<CameraController>();
        if (!cameraController) { Log.Warning("didn't find cameracontroller component"); return; }
        GameObject.Destroy(cameraController);
        HandleNewPelvis(CCPlugin.cutscenePlayer.outfitManager.pelvis);
    }

    void HandleCutsceneEnd(Slate.Cutscene obj)
    {
        Log.Info("InsetCamera.HandleCutsceneEnd()");
        var slate = GetComponent<Slate.GameCamera>();
        if (slate) GameObject.DestroyImmediate(slate);
        this.StartCoroutine(ReenableCamera());
    }

    IEnumerator ReenableCamera()
    {
        if (!camera) yield break;

        camera.enabled = false;
        yield return new WaitForSeconds(0.1f);

        camera.enabled = true;
        yield break;
    }

    void HandleNewPelvis(PelvisWatchdog pelvis)
    {
        cameraPostionDriver = pelvis
            .transform
            .parent;
        cameraTarget = cameraPostionDriver.RecursiveFindTransform(x => x.name == "CarolRibcage");
        if (!cameraTarget || !cameraPostionDriver) { Log.Error("Failed to restore target bones."); return; }
    }

    void LateUpdate()
    {
        if (!camera) return;
        if (!cameraTarget || !cameraPostionDriver) return;

        this.transform.position = (cameraPostionDriver.position + 3 * cameraPostionDriver.transform.forward);
        this.transform.LookAt(cameraTarget);
    }

    public IEnumerator Save(string filePath)
    {
        yield return new WaitForEndOfFrame();

        var black = Capture(Color.black);
        var white = Capture(Color.white);
        var alpha = CalculateTransparency(black, white);
        byte[] bytes = alpha.EncodeToPNG();
        Log.Debug("Encode complete");

        File.WriteAllBytes(filePath, bytes);
        var descriptor = new RecipeDescriptor23(CCPlugin.cutscenePlayer.outfitManager);
        string json = JsonConvert.SerializeObject(descriptor, Formatting.None);
        PngMetadataUtil.AddMetadata(filePath, Constants.PNGChunkKeyword, json);
        Log.Info("Save complete.");
    }

    Texture2D Capture(Color color)
    {
        camera.enabled = true;
        RenderTexture.active = camera.targetTexture ??= new(TextureSize, TextureSize, 32);
        camera.backgroundColor = color;
        camera.Render();
        Log.Debug("Render Complete");
        Texture2D result = new Texture2D(
            camera.targetTexture.width,
            camera.targetTexture.height,
            TextureFormat.RGBA32, false);
        if (result) Log.Info("created texture2d");

        result.ReadPixels(
            new Rect(
                0, 0,   
                camera.targetTexture.width,
                camera.targetTexture.height),
            0, 0);

        result.Apply();
        Log.Debug("ReadPixels complete");
        camera.enabled = false;
        return result;
    }

    Texture2D CalculateTransparency(Texture2D blackTexture, Texture2D whiteTexture)
    {
        const int mipmap = 0;
        Texture2D alphaTex = new Texture2D(
            camera.targetTexture.width,
            camera.targetTexture.height,
            TextureFormat.RGBA32, false);
        var black = blackTexture.GetPixelData<Color32>(mipmap);
        var white = whiteTexture.GetPixelData<Color32>(mipmap);
        var alpha = black
            .Zip(white, 
                (black, white) => 
                white.DifferenceToAlpha(black))
            .ToArray();
        alphaTex.SetPixelData(alpha, mipmap);
        return alphaTex;
    }

    void OnDestroy() => Slate.Cutscene.OnCutsceneStopped -= HandleCutsceneEnd;
}
