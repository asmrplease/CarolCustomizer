using CarolCustomizer;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Utils;
using Newtonsoft.Json;
using System.IO;
using System.Collections;
using UnityEngine;
using System.Linq;
using CarolCustomizer.Behaviors.Carol;

namespace FaceCam.Behaviors;
public class ThumbnailCamera : MonoBehaviour
{
    readonly Vector3 cameraRotationAxis = new(0, 1, 0);
    Camera camera;
    CameraController cameraController;
    Transform cameraPostionDriver;
    Transform cameraTarget;
    

    void Awake()
    {
        Log.Debug("InsetCamera.Awake()");
        camera = this.GetComponent<Camera>();
        if (!camera) { Log.Error("InsetCamera instantiated on an object without a camera component!"); return; }

        this.name = "Thumbnail Camera";
        var collider = GetComponent<SphereCollider>();
        if (collider) GameObject.Destroy(collider);

        camera.cullingMask = 1 << Constants.SMRLayer;
        camera.clearFlags = CameraClearFlags.Color; //CameraClearFlags.Depth;//
        camera.backgroundColor = Constants.DefaultColor;
        camera.depth = Camera.main.depth + 1;
        camera.fieldOfView = 18;
        camera.aspect = 1;
        camera.useOcclusionCulling = false;
        camera.enabled = false;

        PlayerInstances.DefaultPlayer.SpawnEvent += HandleNewPelvis;
        Slate.Cutscene.OnCutsceneStopped += HandleCutsceneEnd;

        cameraController = this.GetComponent<CameraController>();
        if (!cameraController) { Log.Warning("didn't find cameracontroller component"); return; }
        GameObject.Destroy(cameraController);
        HandleNewPelvis(PlayerInstances.DefaultPlayer.outfitManager.pelvis);
    }

    void HandleCutsceneEnd(Slate.Cutscene obj)
    {
        var slate = GetComponent<Slate.GameCamera>();
        if (slate) GameObject.DestroyImmediate(slate);
    }

    void HandleNewPelvis(PelvisWatchdog pelvis)
    {
        cameraPostionDriver = pelvis
            .transform
            .parent;
        cameraTarget = cameraPostionDriver.RecursiveFindTransform(x => x.name == "CarolRibcage");
        if (!cameraTarget || !cameraPostionDriver) { Log.Error("Failed to restore target bones."); return; }
    }

    public IEnumerator Save(string filePath)
    {
        yield return new WaitForEndOfFrame();

        this.transform.position = (cameraPostionDriver.position + 3 * cameraPostionDriver.transform.forward);
        this.transform.LookAt(cameraTarget);
        var black = Capture(Color.black);
        var white = Capture(Color.white);
        var alpha = CalculateTransparency(black, white);
        byte[] bytes = alpha.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);
        var descriptor = new LatestDescriptor(PlayerInstances.DefaultPlayer.outfitManager);
        string json = JsonConvert.SerializeObject(descriptor, Formatting.None);
        PngMetadataUtil.AddMetadata(filePath, Constants.PNGChunkKeyword, json);
        Log.Info("Save complete.");
    }

    Texture2D Capture(Color color)
    {
        camera.enabled = true;
        RenderTexture.active = 
            camera.targetTexture 
            ??= new(Constants.ThumbnailSize, Constants.ThumbnailSize, 32);
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
