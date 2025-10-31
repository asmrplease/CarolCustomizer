using CarolCustomizer.Utils;
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace CarolCustomizer.Models.Materials;
[Serializable]
public class MaterialDescriptor : IEquatable<MaterialDescriptor>, IComparable<MaterialDescriptor>
{
    public string Name;
    public string Source;
    public SourceType Type;

    [JsonIgnore]
    public readonly Material referenceMaterial;

    [JsonConstructor]
    public MaterialDescriptor(string name, string source, SourceType type)
    {
        Name = name;
        Source = source;
        Type = type;
        referenceMaterial = null;
    }

    public MaterialDescriptor(Material material, string sourceName, SourceType type)
    {
        referenceMaterial = material;
        Source = sourceName;
        Name = material.name;
        Type = type;
    }

    public enum SourceType
    {
        AssetBundle = 0,
        World = 1,
        Resources = 2,
        Hair = 3,
    }

    public bool Equals(MaterialDescriptor other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Type == other.Type 
            && Name.DeInstance() == other.Name.DeInstance() 
            && Source == other.Source;
    }

    public override bool Equals(object other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != GetType()) return false;

        return Equals((MaterialDescriptor)other);
    }

    public override int GetHashCode()
    {
        return Type.GetHashCode() ^ Name.DeInstance().GetHashCode() ^ Source.GetHashCode();
    }

    public int CompareTo(MaterialDescriptor other)
    {
        //var scene = this.Source.CompareTo(other.Source);
        //if (scene != 0) return scene;
        var name = this.Name.CompareTo(other.Name);
        return name;
    }
}
