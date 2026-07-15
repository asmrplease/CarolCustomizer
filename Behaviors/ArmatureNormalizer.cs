using CarolCustomizer.Assets;
using CarolCustomizer.Utils;
using UnityEngine;

namespace CarolCustomizer.Behaviors;
internal class ArmatureNormalizer
{
    public static void Initialize()
    {

    }

    public static int FindAndRenameAll(GameObject pelvis)
    {

        return -1;
    }

    void FindMisnamedBone(string expectedName)
    {
        if (expectedName == Constants.Pelvis) { } //base case for recursive search up the armature, can't fix a missing pelvis lol
        //get the name of the expected bone's parent

        OutfitAssetManager
            .GetOutfitByAssetName(Constants.Pyjamas)
            .boneData
            .StandardBones
    }

}
