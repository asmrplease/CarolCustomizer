using CarolCustomizer.Assets;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace CarolCustomizer.Models.Accessories;

internal partial class MutableModel
{
    AccessoryDescriptor target;
    List<MutableMaterial> materials;

    public MutableModel(AccessoryDescriptor target)
    {
        this.target = target;
        this.materials = target.Materials
            .Select((x, i) => new MutableMaterial(x, target, i))
            .ToList();
    }
}

internal partial class MutableModel : IListable
{
    IListable idk => target as IListable;
    public Sprite Thumbnail => idk.Thumbnail;

    public string Header => idk.Header;

    public string Subheader => idk.Subheader;

    public Color BaseColor => Constants.DefaultColor;

    public Color HighlightColor => Constants.Highlight;

    public IEnumerable<IListable> Children => this.materials;

    public UnityAction<bool> OnToggle => (visible) => PlayerInstances.DefaultPlayer.outfitManager.SetAccessory(this.target, visible);

    public bool Filter<T>(Predicate<T> predicate)
    {
        throw new NotImplementedException();
    }

    public List<(string, UnityAction)> GetContextMenuItems()
    {
        var results = idk.GetContextMenuItems();
        //add any mutable-only operations here.
        return results;
    }
}

