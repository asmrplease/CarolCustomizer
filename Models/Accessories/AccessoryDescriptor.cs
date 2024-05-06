using CarolCustomizer.Utils;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace CarolCustomizer.Models.Accessories;
[Serializable]
public class AccessoryDescriptor : IEquatable<AccessoryDescriptor>
{
    public readonly string Name;
    public readonly string Source;
    public MaterialDescriptor[] Materials;

    [JsonConstructor]
    public AccessoryDescriptor(string name, string source, MaterialDescriptor[] materials)
    {
        Name = name;
        Source = source;
        Materials = materials;
    }

    public AccessoryDescriptor(string name, string source)
    {
        Name = name;
        Source = source;
        //does having an empty list of materials cause problems here?
        //only thing that uses this is LiveAccessory
    }

    public AccessoryDescriptor(SkinnedMeshRenderer smr, string source)
    {
        Name = smr.name;
        Source = source;
        Materials = new MaterialDescriptor[smr.materials.Length];

        int index = 0;
        foreach (var material in smr.materials)
        {
            Materials[index++] = new(material, source, MaterialDescriptor.SourceType.AssetBundle);
        }
    }

    public AccessoryDescriptor(AccessoryDescriptor existing)
    {
        Name = existing.Name;
        Source = existing.Source;
        Materials = existing.Materials;
    }

    public bool Equals(AccessoryDescriptor other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Name.DeInstance() == other.Name.DeInstance() 
            && Source == other.Source 
            && Materials.SequenceEqual(other.Materials);
    }

    public override bool Equals(object other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != GetType()) return false;

        return Equals((AccessoryDescriptor)other);
    }

    public static bool operator ==(AccessoryDescriptor left, AccessoryDescriptor right) => Equals(left, right);
    public static bool operator !=(AccessoryDescriptor left, AccessoryDescriptor right) => !Equals(left, right);

    public override int GetHashCode()
    {
        var mats = Materials as IStructuralEquatable;
        var matsHash = mats.GetHashCode(EqualityComparer<MaterialDescriptor>.Default);
        unchecked { return Name.DeInstance().GetHashCode() << 12 ^ Source.GetHashCode() << 8 ^ matsHash; }
    }

    public override string ToString() => $"AD:{Source}.{Name}";

}
