using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Hooks;
using CarolCustomizer.Models.Accessories;
using UnityEngine;

namespace CarolCustomizer.Contracts;
internal interface IInstantiable
{
    LiveAccessory MakeLive(SkeletonManager skeleton, FaceCopier faceCopier, Transform folder);

}
