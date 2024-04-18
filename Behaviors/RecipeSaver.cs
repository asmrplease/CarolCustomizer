using Newtonsoft.Json;
using System.IO;
using CarolCustomizer.Utils;
using CarolCustomizer.Models.Recipes;

namespace CarolCustomizer.Behaviors;
internal static class RecipeSaver
{
    public static string RecipeFilenameToPath(string fileName)
    {
        string relativePath = Path.Combine(Constants.RecipeFolderName, fileName);
        string path = Path.Combine(Constants.ApplicationPath, relativePath);
        return path;
    }

    public static void Save(RecipeDescriptor20 recipe, string filePath)
    {
        string json = JsonConvert.SerializeObject(recipe, Formatting.Indented);
        Log.Debug(json);
        if (!filePath.Contains(Constants.RecipeExtension))
        {
            filePath = RecipeFilenameToPath($"{filePath}{Constants.RecipeExtension}");
        }
        var newSave = File.CreateText(filePath);
        newSave.Write(json);
        newSave.Close();
    }
}
