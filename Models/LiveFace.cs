using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CarolCustomizer.Hooks;
using CarolCustomizer.Behaviors;
using CarolCustomizer.Utils;

namespace CarolCustomizer.Models;
public class LiveFace : LiveAccessory
{
    #region Dependencies
    FaceCopier faceCopier;
    #endregion

    #region Behavior
    public LiveFace(StoredAccessory acc, SkeletonManager skeleton, Transform folder) : base(acc, skeleton, folder) 
    {
        this.faceCopier = skeleton.faceCopier;
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
