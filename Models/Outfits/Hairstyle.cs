using UnityEngine;

namespace CarolCustomizer.Models.Outfits;

public class HairstyleOutfit : Outfit
{
	public readonly Hairstyle hairstyle;

	public HairstyleOutfit(Transform storedAsset) : base(storedAsset)
	{
		this.hairstyle = storedAsset.GetComponent<Hairstyle>();

	}
}

//How do we handle hair?
//It seems just different enough from existing systems to not fit nicely into any of them
//Hair has it's own armature and SMR(s?)
//The armature's root bone needs to be instantiated on the head
//It needs to be toggled on and off independantly from the rest of the base model SMRs
//Materials need to be applied to the hair