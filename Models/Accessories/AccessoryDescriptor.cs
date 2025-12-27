using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static ModelData;

namespace CarolCustomizer.Models.Accessories;
[Serializable]
public partial class AccessoryDescriptor 
{
    public readonly string Name;
    public readonly SourceDescriptor Source;
    public MaterialDescriptor[] Materials = [];

    [JsonConstructor]
    public AccessoryDescriptor(string name, SourceDescriptor source, MaterialDescriptor[] materials)
    {
        Name = name;
        Source = source;
        Materials = materials;
    }

    public AccessoryDescriptor(string name, SourceDescriptor source)
    {
        Name = name;
        Source = source;
    }

    public AccessoryDescriptor(SkinnedMeshRenderer smr, SourceDescriptor source)
    {
        if (!smr) Log.Error($"AccessoryDescriptor in {source} was passed a null smr.");

        Name = smr.name;
        Source = source;
        Materials = new MaterialDescriptor[smr.materials.Length];

        int index = 0;
        foreach (var material in smr.materials)
        {
            Materials[index++] = new MaterialDescriptor(material, source);
        }
    }

    public AccessoryDescriptor(AccessoryDescriptor existing)
    {
        Name = existing.Name;
        Source = existing.Source;
        Materials = existing.Materials;
    }

    public override string ToString() => $"AD:{Source}.{Name}";
    public void SetFavorite(bool favorited)
    {
        if (favorited) Settings.Favorites.AddToFavorites(this);
        else Settings.Favorites.RemoveFromFavorites(this);
    }
}

public partial class AccessoryDescriptor : IEquatable<AccessoryDescriptor>
{
    public bool Equals(AccessoryDescriptor other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return this.Name.DeInstance().Equals(other.Name.DeInstance())
            && this.Source.Equals(other.Source);
            //&& this.Materials.SequenceEqual(other.Materials);
    }

    public override bool Equals(object other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != GetType()) return false;

        return Equals((AccessoryDescriptor)other);
    }

    public static bool operator ==(AccessoryDescriptor left, AccessoryDescriptor right) => Equals(left, right);
    public static bool operator !=(AccessoryDescriptor left, AccessoryDescriptor right) => !Equals(left, right);

    public override int GetHashCode()
    {
        //var mats = Materials as IStructuralEquatable;
        //var matsHash = mats.GetHashCode(EqualityComparer<MaterialDescriptor>.Default);
        unchecked { return Name.DeInstance().GetHashCode() << 12 ^ Source.GetHashCode() << 8; }// ^ matsHash; }
    }
}

public partial class AccessoryDescriptor : IListable
{
    Sprite IListable.Thumbnail => null;

    string IListable.Header => this.Name;

    string IListable.Subheader => $"{this.Materials.Length} materials";

    Color IListable.BaseColor => Constants.DefaultColor;

    Color IListable.HighlightColor => Constants.Highlight;

    IEnumerable<IListable> IListable.Children => this.Materials;
}

public partial class AccessoryDescriptor : IContextMenuActions
{
    List<ContextButton> IContextMenuActions.GetContextMenuItems()
    {
        var results = this.AutoMenuItems();
        bool currentlyFavorite = Settings.Favorites.IsInFavorites(this);
        results.AddRange(
        [
            (currentlyFavorite ? "Remove from Favorites" : "Add to favorites",
            () => SetFavorite(!currentlyFavorite))
        ]);
        return results;
    }
}