using CarolCustomizer.UI;
using CarolCustomizer.Utils;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CarolCustomizer.Models.Accessories;
[Serializable]
public class AccessoryDescriptor : IEquatable<AccessoryDescriptor>
{
    public string Name;
    public string Source;
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

        return Name.DeInstance() == other.Name.DeInstance() && Source == other.Source && Materials.SequenceEqual(other.Materials);
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
