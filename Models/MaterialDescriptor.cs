using CarolCustomizer.Utils;
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace CarolCustomizer.Models;
[Serializable]
public class MaterialDescriptor : IEquatable<MaterialDescriptor> 
{
    public string Name;
    public string Source;
    public SourceType Type;

    [JsonIgnore]
    public readonly Material referenceMaterial;
    
    [JsonConstructor]
    public MaterialDescriptor(string name, string source, SourceType type)
    {
        this.Name = name;
        this.Source = source;
        this.Type = type;
        this.referenceMaterial = null;
    }

    public MaterialDescriptor(Material material, string sourceName, SourceType type)
    {
        this.referenceMaterial = material;
        this.Source = sourceName;
        this.Name = material.name;
        this.Type = type;
    }

    public enum SourceType
    {
        AssetBundle = 0,
        World = 1
    }

    public bool Equals(MaterialDescriptor other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return (this.Type == other.Type && this.Name.DeInstance() == other.Name.DeInstance() && this.Source == other.Source);
    }

    public override bool Equals(object other)
    {
        if (ReferenceEquals(null, other))       return false;
        if (ReferenceEquals(this, other))       return true;
        if (other.GetType() != this.GetType())  return false;

        return this.Equals((MaterialDescriptor)other);
    }

    public override int GetHashCode()
    {
        return this.Type.GetHashCode() ^ this.Name.DeInstance().GetHashCode() ^ this.Source.GetHashCode();
    }
}
