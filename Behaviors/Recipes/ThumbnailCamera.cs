using CarolCustomizer;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Utils;
using Newtonsoft.Json;
using System.IO;
using System.Collections;
using UnityEngine;

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
        camera.cullingMask = 1 << Constants.SMRLayer;
        camera.clearFlags = CameraClearFlags.SolidColor; //CameraClearFlags.Color;//
        camera.backgroundColor = Constants.DefaultColor;
        camera.depth = Camera.main.depth + 1;
        camera.fieldOfView = 24;
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

        Camera camOV = camera;
        camera.enabled = true;
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = camOV.targetTexture ??= new(TextureSize, TextureSize, 24);

        camOV.Render();
        Log.Debug("Render Complete");
        Texture2D imageOverview = new Texture2D(
            camOV.targetTexture.width,
            camOV.targetTexture.height,
            TextureFormat.ARGB32, false);
        if (imageOverview) Log.Info("created texture2d");

        imageOverview.ReadPixels(
            new Rect(
                0, 0,
                camOV.targetTexture.width,
                camOV.targetTexture.height),
            0, 0);

        imageOverview.Apply();
        RenderTexture.active = currentRT;
        Log.Debug("ReadPixels complete");

        byte[] bytes = imageOverview.EncodeToPNG();
        camera.enabled = false;
        Log.Debug("Encode complete");

        File.WriteAllBytes(filePath, bytes);

        var descriptor = new RecipeDescriptor23(CCPlugin.cutscenePlayer.outfitManager);
        string json = JsonConvert.SerializeObject(descriptor, Formatting.None);
        PngMetadataUtil.AddMetadata(filePath, Constants.PNGChunkKeyword, json);
        Log.Info("Save complete.");
    }

    void OnDestroy() => Slate.Cutscene.OnCutsceneStopped -= HandleCutsceneEnd;
}
