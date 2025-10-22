using UnityEngine;
using CarolCustomizer.Hooks;
using CarolCustomizer.Utils;
using CarolCustomizer.Behaviors.Carol;

namespace CarolCustomizer.Models.Accessories;
public class LiveFace : LiveAccessory
{
    #region Dependencies
    FaceCopier faceCopier;
    #endregion

    #region Behavior
    public LiveFace(StoredAccessory acc, SkeletonManager skeleton, FaceCopier faceCopier, Transform folder) : base(acc, folder)
    {
        this.faceCopier = faceCopier;
    }

    public override void Enable()
    {
        base.Enable();
        faceCopier.AddTarget(this);
    }

    public override void Disable()
    {
        base.Disable();
        faceCopier.RemoveTarget(this);
    }

    public void SetAllBlendshapes(float[] blendshapes) => liveSMR.SetAllBlendshapes(blendshapes);
    #endregion
}
