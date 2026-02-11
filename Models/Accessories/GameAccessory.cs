using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Contracts;
using CarolCustomizer.Hooks;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarolCustomizer.Models.Accessories;
public partial class GameAccessory
{
    public readonly Accessory gameAcc;
    public static SourceDescriptor Source = new(Constants.GameAccessorySourceName, SourceType.GameAcc);
    public bool isSkinned => gameAcc.attachType == Accessory.AccessoryChildTypes.SkinnedToCharacter;

    public GameAccessory(Accessory gameAcc)
    {
        this.gameAcc = gameAcc;
    }

    public string GetParentBoneName() => gameAcc.GetParentBoneName();
}

public partial class GameAccessory : IInstantiable
{
    public AccessoryDescriptor Descriptor => new(this.gameAcc.name, GameAccessory.Source);

    public ILiveModel MakeLive(SkeletonManager skeleton, FaceCopier faceCopier, Transform folder)
    {
        //if (reference.isSkinned) return new LiveAccessory(gameAcc.skinnedMesh);
        return new LiveGameAccessory(this);
    }
}

public partial class LiveGameAccessory
{
    readonly GameAccessory reference;
    GameObject instance;
    PelvisWatchdog watchdog;
    public LiveGameAccessory(GameAccessory acc)
    {
        this.reference = acc;
    }
    
}

public partial class LiveGameAccessory : ILiveModel
{
    public bool isActive { get; private set; } = false;

    AccessoryDescriptor ILiveModel.Descriptor => reference.Descriptor;

    void ILiveModel.HandleNewArmature(PelvisWatchdog watchdog)
    {
        if (instance) GameObject.Destroy(instance);
        string parentName = this.reference.GetParentBoneName();
        Transform parent = watchdog.BoneData.StandardBones[parentName];
        instance = GameObject.Instantiate(this.reference.gameAcc.gameObject, parent);
    }

    void ILiveModel.ApplyMaterial(MaterialDescriptor material, int index)
    {
        //throw new NotImplementedException();
        Log.Warning("GameAccessory.ApplyMaterial Not Implemented.");
    }

    void ILiveModel.ApplySharedMaterials(List<Material> materials)
    {
        //throw new NotImplementedException();
    }

    void ILiveModel.Disable()
    {
        if (!instance) return;
        
        isActive = true;
        instance.SetActive(false);
    }

    void IDisposable.Dispose()
    {
        if (instance) GameObject.Destroy(instance);
    }

    void ILiveModel.Enable()
    {
        if (!instance) return;
        
        isActive = true;
        instance.SetActive(false);
    }

    bool ILiveModel.IsOnArmature(Transform root)
    {
        if (!instance || !root) return false;

        return instance.transform.parent.IsChildOf(root);
    }

    void ILiveModel.SetVisible(bool visible)
    {
        if (!instance) return;

        instance.SetActive(visible);
    }
}
