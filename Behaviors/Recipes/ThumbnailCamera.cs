using CarolCustomizer.Assets;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Utils;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine;

namespace CarolCustomizer.Behaviors.Recipes;
public class ThumbnailCamera : MonoBehaviour
{
    readonly Vector3 cameraRotationAxis = new(0, 1, 0);
    public Camera camera;
    CameraController cameraController;
    Transform cameraPostionDriver;
    Transform cameraTarget;

    void Awake()
    {
        Log.Debug("ThumbnailCamera.Awake()");
        camera = this.GetComponent<Camera>();
        if (!camera) { Log.Error("ThumbnailCamera instantiated on an object without a camera component!"); return; }

        this.name = "Thumbnail Camera";
        camera.cullingMask = 1 << Constants.SMRLayer;
        camera.clearFlags = CameraClearFlags.Color;
        camera.backgroundColor = Constants.DefaultColor;
        camera.depth = Camera.main.depth + 1;
        camera.fieldOfView = 18;
        camera.aspect = 1;
        camera.useOcclusionCulling = false;
        camera.enabled = false;

        PlayerInstances.DefaultPlayer.SpawnEvent += HandleNewPelvis;
        if (PlayerInstances.DefaultPlayer.outfitManager.pelvis is PelvisWatchdog existing) HandleNewPelvis(existing);
        Slate.Cutscene.OnCutsceneStopped += HandleCutsceneEnd;

        if (GetComponent<HxVolumetricCamera>()      is HxVolumetricCamera hxvc)         { Destroy(hxvc); }
        if (GetComponent<HxVolumetricImageEffect>() is HxVolumetricImageEffect hxvie)   { Destroy(hxvie); }
        if (GetComponent<SphereCollider>()   is SphereCollider c)                       { Destroy(c); }
        if (GetComponent<AudioListener>()    is AudioListener a)                        { Destroy(a); }
        if (GetComponent<CameraController>() is CameraController cc)                    { Destroy(cc); }

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

        var res = Constants.ThumbnailSize;
        this.transform.position = (cameraPostionDriver.position + 3 * cameraPostionDriver.transform.forward);
        this.transform.LookAt(cameraTarget);
        var texture = camera.CaptureAlpha(res);
        var bytes = texture.GetPixelData<byte>(0).ToArray();
        var descriptor = new RecipeDescriptor(PlayerInstances.DefaultPlayer.outfitManager);
        string json = JsonConvert.SerializeObject(descriptor, Formatting.None);
        PngUtil.BuildPng(filePath, res, bytes, [(Constants.PNGChunkKeyword, json)]);
        Log.Info("Save complete.");
    }

    void OnDestroy() => Slate.Cutscene.OnCutsceneStopped -= HandleCutsceneEnd;
}
