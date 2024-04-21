using CarolCustomizer.Utils;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Models.Outfits;

/// <summary>
/// Represents one heirarchy of bones that are unique to an outfit.
/// </summary>
public class BespokeBone
{
    #region Static Parent Folder
    private static Transform cleanFolder;
    public static void SetCleanFolder(Transform folder)
    {
        if (cleanFolder) { Log.Error("Tried to reset clean folder"); return; }
        if (!folder) return;

        cleanFolder = folder;
    }
    #endregion

    #region Dependencies
    public readonly Transform referenceBone;
    #endregion

    #region Instance Data
    public readonly Transform cleanedBone;
    #endregion

    #region Constructor
    public BespokeBone(Transform referenceBone)
    {
        this.referenceBone = referenceBone;

        cleanedBone = UnityEngine.Object.Instantiate(referenceBone, cleanFolder);
        var unusualComponents = cleanedBone.GetComponentsInChildren<Component>(true);
        foreach (var component in unusualComponents.
            Where(x => x.GetType() != typeof(Transform)
                    && x.GetType() != typeof(DynamicBone)
                    && x.GetType() != typeof(RectTransform)
                 ))
        {
            UnityEngine.Object.Destroy(component);
        }
    }
    #endregion
}
