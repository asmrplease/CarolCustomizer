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

    public MaterialDescriptor SetClipboard() => MaterialManager.clipboard = this;
}

public partial class MaterialDescriptor : IEquatable<MaterialDescriptor>
{
    public bool Equals(MaterialDescriptor other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return this.GetHashCode() == other.GetHashCode();
        //TODO: why did the below fail to return equals when the hashcode was equal?
        //return Name.DeInstance() == other.Name.DeInstance() 
        //    && Source == other.Source;
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
    Sprite IListable.Thumbnail => throw new NotImplementedException();

    string IListable.Header => throw new NotImplementedException();

    string IListable.Subheader => throw new NotImplementedException();

    Color IListable.BaseColor => throw new NotImplementedException();

    Color IListable.HighlightColor => throw new NotImplementedException();

    bool IListable.Filter<T>(Predicate<T> predicate)
    {
        throw new NotImplementedException();
    }

    IEnumerable<IListable> IListable.Children => [];

    public List<(string, UnityAction)> GetContextMenuItems()
    {
        return
        [
            ("Copy", () => this.SetClipboard()),
            //("Copy Current Material", CopyCurrentMaterial),
            //("Paste Material", PasteMaterial),
            //("Reset Material", ResetMaterial)
        ];
        
    }
}