using CarolCustomizer.Utils;
using MagicaCloth2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace CarolCustomizer.Models.Outfits;

public class HairData
{
	public readonly Hairstyle hairstyle;
	public readonly MagicaCloth physics;
	public readonly Transform armatureRoot;
	public readonly List<SkinnedMeshRenderer> models;

	public HairData(Transform storedAsset) 
	{
		this.hairstyle	  = storedAsset.GetComponent<Hairstyle>();
		this.models		  = storedAsset.GetComponentsInChildren<SkinnedMeshRenderer>(true).ToList();
		this.physics	  = storedAsset.GetComponentInChildren<MagicaCloth>();
		this.armatureRoot = storedAsset.RecursiveFindTransform(x => x.name == "Carol_HairRoot");
	}
}

//How do we handle hair?
//It seems just different enough from existing systems to not fit nicely into any of them
//Hair has it's own armature and SMR(s?)
//The armature's root bone needs to be instantiated on the head
//It needs to be toggled on and off independantly from the rest of the base model SMRs
//Materials need to be applied to the hair

//we could always add hair to the player armature for each outfit 