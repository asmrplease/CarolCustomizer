using Newtonsoft.Json;
using System.IO;
using CarolCustomizer.Utils;
using CarolCustomizer.Models.Recipes;

namespace CarolCustomizer.Behaviors.Recipes;
internal static class RecipeSaver
{
    public static string RecipeFilenameToPath(string fileName)
    {
        string relativePath = Path.Combine(Constants.RecipeFolderName, fileName);
        string path = Path.Combine(Constants.ApplicationPath, relativePath);
        return path;
    }

    public static void SaveJson(RecipeDescriptor23 recipe, string filePath)
    {
        string json = JsonConvert.SerializeObject(recipe, Formatting.Indented);
        Log.Debug(json);
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
