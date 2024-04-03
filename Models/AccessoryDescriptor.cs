using CarolCustomizer.UI;
using CarolCustomizer.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CarolCustomizer.Models;
[Serializable]
public class AccessoryDescriptor :IEquatable<AccessoryDescriptor>
{
    public string Name;
    public string Source;
    public MaterialDescriptor[] Materials;

    [JsonConstructor]
    public AccessoryDescriptor(string name, string source, MaterialDescriptor[] materials)
    {
        this.Name = name;
        this.Source = source;
        this.Materials = materials;
    }

    public AccessoryDescriptor(string name, string source)
    {
        this.Name = name;
        this.Source = source;
    }

    public AccessoryDescriptor(AccessoryDescriptor existing)
    {
        this.Name = existing.Name;
        this.Source = existing.Source;
        this.Materials = existing.Materials;
    }

    public bool Equals(AccessoryDescriptor other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return this.Name == other.Name && this.Source == other.Source && this.Materials == other.Materials;
    }

    public override bool Equals(object other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != this.GetType()) return false;

        return this.Equals((AccessoryDescriptor)other);
    }

    public static bool operator ==(AccessoryDescriptor left, AccessoryDescriptor right) => Equals(left, right);
    public static bool operator !=(AccessoryDescriptor left, AccessoryDescriptor right) => !Equals(left, right);

    public override int GetHashCode()
    {
        unchecked { return this.Name.GetHashCode() << 12  ^ this.Source.GetHashCode() << 8 ^ this.Materials.GetHashCode(); }
    }

    public override string ToString() => $"AD:{Source}.{Name}";

}
