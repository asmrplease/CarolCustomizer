using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace CarolCustomizer.Models.Materials;
[Serializable]
public class MaterialDescriptor : IEquatable<MaterialDescriptor>
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

    public override string ToString() => $"MD:{Source}.{Name}";
}
