using Newtonsoft.Json;
using System.IO;
using CarolCustomizer.Utils;
using CarolCustomizer.Models.Recipes;

namespace CarolCustomizer.Behaviors.Recipes;
internal static class RecipeSaver
{
    /// <summary>
    /// Combines the application path, recipe folder path, and given filename.
    /// </summary>
    /// <param name="fileName">File name with extension.</param>
    /// <returns>Path to the root recipe folder with filename included.</returns>
    public static string RecipeFilenameToPath(string fileName)
    {
        string relativePath = Path.Combine(Constants.RecipeFolderName, fileName);//TODO: this doesn't support recipes in subfolders :<
        string path = Path.Combine(Constants.ApplicationPath, relativePath);
        return path;
    }

    public static void SaveJson(RecipeDescriptor23 recipe, string filePath)
    {
        string json = JsonConvert.SerializeObject(recipe, Formatting.Indented);
        //Log.Debug(json);
        if (!filePath.Contains(Constants.JsonFileExtension))
        {
            filePath = RecipeFilenameToPath($"{filePath}{Constants.JsonFileExtension}");
        }
        var newSave = File.CreateText(filePath);
        newSave.Write(json);
        newSave.Close();
    }

    public static void SavePNG(RecipeDescriptor23 recipe, string filePath)
    {
        string json = JsonConvert.SerializeObject(recipe, Formatting.None);
        if (!filePath.Contains(Constants.PngFileExtension))
        {
            filePath = RecipeFilenameToPath($"{filePath}{Constants.PngFileExtension}");
            filePath = filePath.Replace(Constants.JsonFileExtension, "");
        }
        CCPlugin
            .CoroutineRunner
            .StartCoroutine(CCPlugin
                .thumbnailCamera
                .Save(filePath));
    }
}
