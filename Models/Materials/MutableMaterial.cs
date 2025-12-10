using CarolCustomizer.Contracts;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CarolCustomizer.Models.Materials;

internal partial class MutableMaterial
{
    public readonly MaterialDescriptor DefaultMaterial;
    public MaterialDescriptor ActiveMaterial { get; private set; }

    public MutableMaterial(MaterialDescriptor target)
    {
        this.DefaultMaterial = target;
        this.ActiveMaterial = target;
    }

    public void SetMaterial(MaterialDescriptor newMaterial)
    {
        this.ActiveMaterial = newMaterial;
    }

    public void ResetMaterial()
    {
        this.ActiveMaterial = DefaultMaterial;
    }
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

    UnityAction<bool> IListable.OnToggle => def.OnToggle;

    bool IListable.Filter<T>(Predicate<T> predicate)
    {
        return def.Filter(predicate) || act.Filter(predicate);
    }

    List<(string, UnityAction)> IContextMenuActions.GetContextMenuItems()
    {
        var results = def.GetContextMenuItems();
        results.AddRange(act.GetContextMenuItems());
        results.Add(("Paste Material", () => { }));
        results.Add(("Reset Material", () => { }));
        return results;
    }
}
