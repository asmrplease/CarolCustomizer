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
}

internal partial class MutableMaterial : IContextMenuActions
{
    [MenuItem("Paste Material")]
    void Paste() => this.SetMaterial(MaterialManager.clipboard);

    [MenuItem("Reset Material")]
    public void ResetMaterial() => SetMaterial(this.DefaultMaterial);

    List<ContextButton> IContextMenuActions.GetContextMenuItems()
    {
        var results = (def as IContextMenuActions).GetContextMenuItems();
        results.AddRange((act as IContextMenuActions).GetContextMenuItems());
        results.AddRange(this.AutoMenuItems());
        return results;
    }
}
