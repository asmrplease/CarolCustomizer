using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Materials;
using MagicaCloth2;
using System.Collections.Generic;
using UnityEngine;

namespace CarolCustomizer.Contracts
{
    public interface IAccessorySource
    {
        SourceDescriptor Descriptor { get; }
        StoredAccessory GetAccessory(AccessoryDescriptor accessory);
        MaterialDescriptor GetMaterial(MaterialDescriptor material);
        List<Transform> GetBespokeBones();
        List<MagicaCloth> GetBoneCloths();
    }
}
