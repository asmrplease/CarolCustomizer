using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Outfits;
using System;

namespace CarolCustomizer.Contracts;
public interface ICustomizable : IDisposable
{
    ICustomizable Constructor(PelvisWatchdog watchdog);
    void SetBaseOutfit(Outfit outfit);
    void SetAnimator(Outfit outfit);
    void SetHeightOffset(float height);
    void SetBaseVisibility(bool visibility);

}
