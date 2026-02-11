using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Materials;
using MagicaCloth2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarolCustomizer.Contracts;
public interface ILiveModel : IDisposable
{
    bool isActive { get; }
    AccessoryDescriptor Descriptor { get; }
    void Enable();
    void Disable();
    void HandleNewArmature(PelvisWatchdog watchdog);
    void SetVisible(bool visible);
    void ApplyMaterial(MaterialDescriptor material, int index);
    void ApplySharedMaterials(List<Material> materials);
    bool IsOnArmature(Transform root);
}

public interface ISkinned : ILiveModel
{
    List<Transform> BespokeBones { get; }
    Transform[] ReferenceBones { get; }
    string RootBoneName { get; }
    void SetLiveBones(Transform[] liveBones, Transform rootBone);

}

public interface ILiveMagica : ILiveModel
{
    IBoneProvider boneProvider { get; }
    MagicaCloth meshCloth { get; }
    void AddToMagica(MagicaCloth magica);
}