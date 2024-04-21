using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Models.Outfits;
public class MeshData : MonoBehaviour
{
    [SerializeField]
    public List<SkinnedMeshRenderer> baseMeshes;

    public SkinnedMeshRenderer BaseFace => baseMeshes.FirstOrDefault(x => x.name == "tete");

    public MeshData Constructor()
    {
        baseMeshes = transform.parent.GetComponentsInChildren<SkinnedMeshRenderer>(true).ToList();
        return this;
    }
}
