using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Contracts;
using CarolCustomizer.Hooks;
using CarolCustomizer.Models.Materials;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarolCustomizer.Models.Accessories;
public partial class GameAccessory
{
    readonly Accessory gameAcc;
    bool isSkinned => gameAcc.attachType == Accessory.AccessoryChildTypes.SkinnedToCharacter;

    public GameAccessory(Accessory gameAcc)
    {
        this.gameAcc = gameAcc;
    }
}

public partial class GameAccessory : IInstantiable
{
    public AccessoryDescriptor Descriptor => throw new NotImplementedException();

    public ILiveModel MakeLive(SkeletonManager skeleton, FaceCopier faceCopier, Transform folder)
    {
        return new LiveGameAccessory(this, skeleton);

    }
}

public partial class LiveGameAccessory
{
    readonly MeshRenderer liveMesh;

    public LiveGameAccessory(GameAccessory acc, SkeletonManager skeleton)
    {
        
    }
}

public partial class LiveGameAccessory : ILiveModel
{
    public bool isActive { get; private set; } = false;

    AccessoryDescriptor ILiveModel.Descriptor => throw new NotImplementedException();

    void ILiveModel.ApplyMaterial(MaterialDescriptor material, int index)
    {
        throw new NotImplementedException();
    }

    void ILiveModel.ApplySharedMaterials(List<Material> materials)
    {
        throw new NotImplementedException();
    }

    void ILiveModel.Disable()
    {
        throw new NotImplementedException();
    }

    void IDisposable.Dispose()
    {
        throw new NotImplementedException();
    }

    void ILiveModel.Enable()
    {
        throw new NotImplementedException();
    }

    bool ILiveModel.IsOnArmature(Transform root)
    {
        throw new NotImplementedException();
    }

    void ILiveModel.SetVisible(bool visible)
    {
        throw new NotImplementedException();
    }
}
