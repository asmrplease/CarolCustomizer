using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Outfits;
using System;
using UnityEngine;

namespace CarolCustomizer.Contracts;
public interface ICarolType : IDisposable
{
    ICarolType Constructor(PelvisWatchdog watchdog);
    void SetBaseOutfit(SourceDescriptor outfit);
    void SetAnimator(RuntimeAnimatorController rac);
    void SetHeightOffset(float height);
    void SetBaseVisibility(bool visibility);

}
