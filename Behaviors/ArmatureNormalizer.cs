using CarolCustomizer.Assets;
using CarolCustomizer.Utils;
using UnityEngine;

namespace CarolCustomizer.Behaviors;
internal class ArmatureNormalizer
{
    public static bool FindAndRename(Transform pelvis, string expectedName)
    {
        if (FindMisnamedBone(pelvis, expectedName) is not Transform foundBone) { Log.Warning($"Unable to find missing standard bone {expectedName}"); return false; }
        
        foundBone.name = expectedName;
        Log.Debug($"Renamed {foundBone.name} to {expectedName}.");
        return true;
    }

    static Transform FindMisnamedBone(Transform pelvis, string expectedName)
    {
        if (expectedName == Constants.Pelvis) { Log.Warning($"Tried to find misnamed pelvis."); return null; } //base case for recursive search up the armature, can't fix a missing pelvis lol
        if (!CommonBones.IsCommon(expectedName)) { Log.Warning($"Tried to find a non-standard bone {expectedName}"); return null; }

        //get the name of the expected bone's parent
        var referenceBone = OutfitAssetManager
            .GetOutfitByAssetName(Constants.Pyjamas)
            .boneData
            .StandardBones[expectedName];
        var parentName = referenceBone
            .parent
            .name;
        if (!CommonBones.IsCommon(parentName)) { Log.Warning($"Tried to find {expectedName}'s parent {parentName}, but that is not a standard bone name."); return null; }

        //try to find the parent on the pelvis
        var result = pelvis.RecursiveFindTransform(x => x.name == parentName);
        if (result is not Transform parent) { Log.Warning($"Failed to find parent bone {parentName}"); return null; }

        //confirm parent has only one child
        if (parent.childCount == 0) { Log.Warning($"{expectedName}'s parent {parentName} had no children."); return null; }
        //could we simply instantiate the missing bone if it isn't present?

        Transform found = null;
        if (parent.childCount < 1) { Log.Warning("parent bone had no children."); return null; }
        if (parent.childCount == 1) found = parent.GetChild(0);
        if (parent.childCount > 1) 
        {
            if (!parent.name.EndsWith("Palm")) { return null; }
            //Log.Warning($"{expectedName}'s parent {parentName} had too many children to infer which is the correct bone."); return null; 
            var index = referenceBone.GetSiblingIndex();
            if (parent.childCount < index) { Log.Warning("parent had too few children to match sibling index."); return null; }

            found = parent.GetChild(index);
        }

        if (found is null) { Log.Error("Failed to "); return null; }

        Log.Debug($"Found {found.name} where {expectedName} should be. ");
        return found;
    }

}
