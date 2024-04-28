using System;
using System.IO;
using System.Linq;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Utils;
using MonoMod.Utils;
using Newtonsoft.Json;

namespace CarolCustomizer.Behaviors.Recipes;
internal static class RecipeLoader
{
    public static string[] GetRecipeFilePaths()
    {
        return Directory.GetFiles(Constants.RecipeFolderPath, $"*{Constants.RecipeExtension}", SearchOption.AllDirectories);
    }

    public static string GetRecipeJson(string path)
    {
        var file = File.OpenText(path);
        if (file is null) { Log.Warning("failed to open file"); return ""; }

        string results = file.ReadToEnd();
        file.Close();
        return results;
    }

    public struct ValidationResults { public Recipe.Status Status; public RecipeDescriptor21 Recipe; }

    public static ValidationResults ValidateRecipeFile(string filePath)
    {
        string json;
        var results = new ValidationResults { Status = Recipe.Status.NoError, Recipe = null };

        try { json = GetRecipeJson(filePath); }
        catch { results.Status = Recipe.Status.FileError; return results; }
        
        Version version;
        try
        {
            version = JsonConvert.DeserializeObject<VersionedObject>(json)?.version;
            version ??= Constants.v100;
        }
        catch { version = Constants.v100; }
        try
        {
            switch (version)
            {
                case var x when x > Constants.v200:
                    Log.Debug("VRF dectected recipe version as > 2.0.0");
                    results.Recipe = JsonConvert.DeserializeObject<RecipeDescriptor21>(json);
                    break;
                default:
                    Log.Debug("Legacy descriptor");
                    results.Recipe = JsonConvert.DeserializeObject<RecipeDescriptor20>(json).ToVersion210();
                    break;
            }
        }
        catch (Exception ex)
        {
            ex.LogDetailed();
            results.Status = Recipe.Status.InvalidJson;
            return results;
        }

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