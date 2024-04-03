using CarolCustomizer.Behaviors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CarolCustomizer.Models;
public class MeshData : MonoBehaviour
{
    [SerializeField]
    public List<SkinnedMeshRenderer> baseMeshes;
    
    public SkinnedMeshRenderer BaseFace => baseMeshes.FirstOrDefault(x => x.name == "tete");

    public MeshData Constructor()
    {
        baseMeshes = this.transform.parent.GetComponentsInChildren<SkinnedMeshRenderer>(true).ToList();
        return this;
    }
}
