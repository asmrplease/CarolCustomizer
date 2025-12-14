using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CarolCustomizer.Models.Materials;

internal partial class MutableMaterial
{
    public readonly MaterialDescriptor DefaultMaterial;
    public readonly (AccessoryDescriptor, int) Model;
    public MaterialDescriptor ActiveMaterial { get; private set; }

    public MutableMaterial(MaterialDescriptor target, AccessoryDescriptor model, int index)
    {
        this.DefaultMaterial = target;
        this.ActiveMaterial = target;
        this.Model = (model, index);
    }

    public void SetMaterial(MaterialDescriptor newMaterial)
    {
        if (newMaterial == ActiveMaterial) return;

        this.ActiveMaterial = newMaterial;
        PlayerInstances.DefaultPlayer.outfitManager
            .PaintAccessory(this.Model.Item1, this.ActiveMaterial, this.Model.Item2);
        OnChange?.Invoke();
    }

    public void ResetMaterial()
    {
        SetMaterial(this.DefaultMaterial);
    }
}

internal partial class MutableMaterial : IUpdateable
{
    public Action OnChange {  get; set; }
}

internal partial class MutableMaterial : IListable
{
    IListable def => this.DefaultMaterial as IListable;
    IListable act => this.ActiveMaterial as IListable;
    Sprite IListable.Thumbnail => def.Thumbnail;

    string IListable.Header => $"Default: {def.Header}";

    string IListable.Subheader => $"Active: {act.Header}";

    Color IListable.BaseColor => Constants.DefaultColor;

    Color IListable.HighlightColor => Constants.Highlight;

    IEnumerable<IListable> IListable.Children => def.Children;

    List<(string, UnityAction)> IContextMenuActions.GetContextMenuItems()
    {
        var results = def.GetContextMenuItems();
        results.AddRange(act.GetContextMenuItems());
        results.Add(("Paste Material", () => this.SetMaterial(MaterialManager.clipboard)));
        results.Add(("Reset Material", () => this.ResetMaterial()));
        return results;
    }
}
