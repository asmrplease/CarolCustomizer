using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CarolCustomizer.Models;
using CarolCustomizer.Utils;
using Newtonsoft.Json;

namespace CarolCustomizer.Behaviors;
internal static class RecipeLoader
{
    public static string[] GetRecipeFilePaths()
    { 
        return Directory.GetFiles(Constants.RecipeFolderPath, $"*{Constants.RecipeExtension}", SearchOption.TopDirectoryOnly);
    }

    public static string GetRecipeJson(string path)
    {
        var file = File.OpenText(path);
        if (file is null) { Log.Warning("failed to open file"); return ""; }

        string results = file.ReadToEnd();
        file.Close();
        return results;
    }

    public struct ValidationResults { public Recipe.Status Status; public RecipeDescriptor Recipe; }

    public static ValidationResults ValidateRecipeFile(string filePath)
    {
        string json;
        var results = new ValidationResults { Status = Recipe.Status.NoError, Recipe = null };
        
        try { json = GetRecipeJson(filePath); }
        catch { results.Status = Recipe.Status.FileError; return results; }

        try { results.Recipe = JsonConvert.DeserializeObject<RecipeDescriptor>(json); }
        catch { results.Status = Recipe.Status.InvalidJson; return results; }
        if (results.Recipe is null) { results.Status = Recipe.Status.InvalidJson; return results; }

        try
        {
            if (RecipeApplier.GetMissingSources(results.Recipe).Any())
            { results.Status = Recipe.Status.MissingSource; }
        }
        catch { results.Status = Recipe.Status.MissingSource; }

        return results;
    }
}