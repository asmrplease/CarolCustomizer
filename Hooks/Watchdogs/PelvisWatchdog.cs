using CarolCustomizer.Assets;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using System;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs;
public class PelvisWatchdog : MonoBehaviour
{
    public event Action<bool> VisibilityChanged;
    public bool Visible => gameObject.activeInHierarchy;

    [SerializeField]
    AnimData animData;
    public AnimData AnimData { get {  return animData; } }

    [SerializeField]
    BoneData boneData;
    public BoneData BoneData { get { return boneData; } }

    [SerializeField]
    CompData compData;
    public CompData CompData { get { return compData; } }

    [SerializeField]
    MagiData magiData;
    public MagiData MagiData { get { return magiData; } }

    public ICarolType Behavior;
    public string parentName => transform.parent?.name ?? "none";
    public string rootName => transform.root?.name ?? "none";

    bool started = false;

    public static PelvisWatchdog GetAddWatchdog(GameObject pelvis)
    {
        if (pelvis.GetComponent<PelvisWatchdog>() is PelvisWatchdog watchdog) return watchdog;
        return pelvis.AddComponent<PelvisWatchdog>();
    }

    public void Awake()
    {
        if (this.started) return;
        this.started = true;

        Log.Debug($"{this.rootName}.{this.parentName}.PelvisWatchdog.Awake()");
        boneData = this.gameObject.AddComponent<BoneData>().Constructor();
        compData = this.gameObject.AddComponent<CompData>().Constructor();
        magiData = this.gameObject.AddComponent<MagiData>().Constructor();
        animData = this.gameObject.AddComponent<AnimData>().Constructor();
        Behavior = this.gameObject.AddComponent<UnknownArmature>();
        ArmatureIdentifier.DetectChanges(this);
    }
    void OnChange()
    {
        ArmatureIdentifier.DetectChanges(this);
        VisibilityChanged?.Invoke(this.Visible);
    }
    void OnEnable() => OnChange();

    void OnDisable() => OnChange();
    void OnTransformParentChanged() 
    {
        //This check prevents issues with situations where the player becomes parented to a vehicle or other object
        if (Behavior.GetType() == typeof(PlayerArmature)) return;

        OnChange();
    }

    void OnDestroy()
    {
        Log.Debug($"{parentName} PelvisWatchdog.Dispose()");
        //List<MonoBehaviour> stuff = [boneData, compData, magiData, animData];
        //Destroy(boneData); Destroy(compData); Destroy(magiData); Destroy(animData);
        //stuff.ForEach(DestroyImmediate);
        //Behavior.Dispose();
    }
}