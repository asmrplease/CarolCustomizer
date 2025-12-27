using CarolCustomizer.Behaviors;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CarolCustomizer.Models.Materials;
[Serializable]
public partial class MaterialDescriptor 
{
    public string Name;
    public SourceDescriptor Source;
    [JsonIgnore]
    public readonly Material referenceMaterial;

    [JsonConstructor]//if we remove this, 2.5 handling breaks
    public MaterialDescriptor(string name, SourceDescriptor source)
    {
        Name = name;
        Source = source;
        referenceMaterial = null;
    }

    public MaterialDescriptor(Material material, string sourceName, SourceType type)
    {
        referenceMaterial = material;
        Source = new SourceDescriptor(sourceName, type);
        Name = material.name;
    }

    public MaterialDescriptor(Material material, SourceDescriptor descriptor)
    {
        referenceMaterial = material;
        Name = material.name;
        Source = descriptor;
    }

    public override string ToString() => $"MD:{Source}.{Name}";
}

public partial class MaterialDescriptor : IEquatable<MaterialDescriptor>
{
    public bool Equals(MaterialDescriptor other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return this.Name.DeInstance().Equals(other.Name.DeInstance())
            && this.Source.Equals(other.Source);
    }

    public static bool operator ==(MaterialDescriptor left, MaterialDescriptor right) => Equals(left, right);
    public static bool operator !=(MaterialDescriptor left, MaterialDescriptor right) => !Equals(left, right);

    public override bool Equals(object other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != GetType()) return false;

        return Equals((MaterialDescriptor)other);
    }

    public override int GetHashCode()
    {
        return Name.DeInstance().GetHashCode() ^ Source.GetHashCode();
    }
}

public partial class MaterialDescriptor : IListable
{
    Sprite IListable.Thumbnail => null;

    string IListable.Header => this.Name;

    string IListable.Subheader => this.referenceMaterial is null? "" : this.referenceMaterial.shader.GetName();

    Color IListable.BaseColor => Constants.DefaultColor;

    Color IListable.HighlightColor => Constants.Highlight;

    IEnumerable<IListable> IListable.Children => [];
}

public partial class MaterialDescriptor : IContextMenuActions
{
    [MenuItem("Copy")]
    void SetClipboard() => MaterialManager.clipboard = this;

    [MenuItem("Debug")]
    void Debug() => Log.Debug($"MenuActions:{this.Name}");

    public List<ContextButton> GetContextMenuItems() => this.AutoMenuItems();
}