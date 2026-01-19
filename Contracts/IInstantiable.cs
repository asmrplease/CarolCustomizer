using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Hooks;
using CarolCustomizer.Models.Accessories;
using UnityEngine;

namespace CarolCustomizer.Contracts;
public interface IInstantiable
{
    AccessoryDescriptor Descriptor { get; }
    ILiveModel MakeLive(SkeletonManager skeleton, FaceCopier faceCopier, Transform folder);

}
