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

    public struct ValidationResults { public Recipe.Status Status; public RecipeDescriptor20 Recipe; }

    public static ValidationResults ValidateRecipeFile(string filePath)
    {
        string json;
        var results = new ValidationResults { Status = Recipe.Status.NoError, Recipe = null };

        try { json = GetRecipeJson(filePath); }
        catch { results.Status = Recipe.Status.FileError; return results; }

        try
        {
            var version = JsonConvert.DeserializeObject<VersionedObject>(json)?.version;
            if (version is null) { results.Status = Recipe.Status.InvalidJson; return results; }

            switch (version)
            {
                case var x when x > Constants.v200:
                    Log.Debug("VRF dectected recipe version as > 2.0.0");
                    results.Recipe = JsonConvert.DeserializeObject<RecipeDescriptor20>(json);
                    break;
                case var x when x == Constants.v200:
                    Log.Debug("VRF dectected recipe version as exactly 2.0.0");
                    results.Recipe = JsonConvert.DeserializeObject<RecipeDescriptor20>(json);
                    break;
                case var x when x == Constants.v100:
                    Log.Debug("VRF recipe version detected as 1.0.0, presumed 2.0.0");
                    results.Recipe = JsonConvert.DeserializeObject<RecipeDescriptor20>(json);
                    break;
                default:
                    Log.Error($"Unable to handle recipe version: {version}.");
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